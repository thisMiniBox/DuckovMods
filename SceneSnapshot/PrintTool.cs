using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace SceneSnapshot
{
    internal class PrintTool : MonoBehaviour
    {
        private const string FOLDER_NAME = "GameObjectSnapshots";
        private const int MAX_REFLECTION_DEPTH = 3; // 最大反射深度，防止循环引用或过深的对象图
        private const int MAX_COLLECTION_ELEMENTS_TO_PRINT = 5; // 集合最多打印的元素数量

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F2)) CaptureAndPrintSceneInfo();
        }

        private void CaptureAndPrintSceneInfo()
        {
            var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var outputFolderPath = Path.Combine(desktopPath, FOLDER_NAME);

            try
            {
                if (!Directory.Exists(outputFolderPath))
                {
                    Directory.CreateDirectory(outputFolderPath);
                    Debug.Log($"创建输出文件夹: {outputFolderPath}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"创建文件夹失败: {outputFolderPath} - {ex.Message}");
                return;
            }

            var activeSceneName = SceneManager.GetActiveScene().name;
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var fileName = $"{activeSceneName}_FullSnapshot_{timestamp}.txt"; // 修改文件名以示区别
            var fullFilePath = Path.Combine(outputFolderPath, fileName);

            var sb = new StringBuilder();

            sb.AppendLine("=================================================");
            sb.AppendLine($"场景信息快照 - 活跃场景: {activeSceneName}");
            sb.AppendLine($"生成时间: {DateTime.Now}");
            sb.AppendLine("=================================================");
            sb.AppendLine();

            sb.AppendLine("--- 鼠标位置对象信息 ---");
            AppendMouseHoverObjectInfo(sb);
            sb.AppendLine();

            sb.AppendLine("--- 所有加载场景的活跃游戏对象层次结构及其组件 ---");

            // 遍历所有已加载的场景
            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                var currentScene = SceneManager.GetSceneAt(i);

                // 打印场景名称作为分割线
                sb.AppendLine($"\n===== 场景: {currentScene.name} ===== " +
                              (currentScene == SceneManager.GetActiveScene() ? "(活跃场景)" : ""));

                GameObject[] rootObjects = currentScene.GetRootGameObjects();
                if (rootObjects.Length == 0)
                    sb.AppendLine("    - 该场景没有根游戏对象。");
                else
                    foreach (var go in rootObjects)
                        AppendGameObjectInfo(go, 0, sb);
            }

            sb.AppendLine("=================================================");


            try
            {
                File.WriteAllText(fullFilePath, sb.ToString(), Encoding.UTF8);
                Debug.Log($"场景信息已成功保存到: {fullFilePath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"保存文件失败: {fullFilePath} - {ex.Message}");
            }
        }

        /// <summary>
        ///     递归地将游戏对象的名称、活跃状态、组件及其子对象的层次结构追加到StringBuilder。
        ///     **注意：此方法只会处理活跃状态为 activeSelf 的对象。**
        /// </summary>
        /// <param name="go">要处理的游戏对象。</param>
        /// <param name="indentLevel">当前缩进级别。</param>
        /// <param name="sb">StringBuilder实例。</param>
        private void AppendGameObjectInfo(GameObject go, int indentLevel, StringBuilder sb)
        {
            // 只有当对象自身是激活状态时才处理和打印
            if (!go || !go.activeSelf) return;

            var indent = new string(' ', indentLevel * 4); // 每个层级使用4个空格缩进

            // 打印游戏对象名称和活跃状态
            sb.AppendLine(
                $"{indent}[{go.name}] (ActiveSelf: {go.activeSelf}, ActiveInHierarchy: {go.activeInHierarchy})");

            // 打印所有组件
            var components = go.GetComponents<Component>();
            foreach (var comp in components)
                if (comp) // 某些组件可能在运行时被销毁
                    sb.AppendLine($"{indent}    - Component: {comp.GetType().Name}");

            // 递归处理子对象
            foreach (Transform child in go.transform) AppendGameObjectInfo(child.gameObject, indentLevel + 1, sb);
        }

        /// <summary>
        ///     尝试检测鼠标位置下方的UI元素或场景对象，并将其路径和组件信息追加到StringBuilder。
        /// </summary>
        /// <param name="sb">StringBuilder实例。</param>
        private void AppendMouseHoverObjectInfo(StringBuilder sb)
        {
            // 首先尝试Raycast UI元素
            var eventDataCurrentPosition = new PointerEventData(EventSystem.current);
            eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            var results = new List<RaycastResult>();
            if (EventSystem.current)
            {
                EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
            }

            if (results.Count > 0)
            {
                // UI元素优先级更高
                var uiObject = results[0].gameObject;
                var uiPath = GetGameObjectPath(uiObject);
                sb.AppendLine($"鼠标下方UI路径: {uiPath}");
                sb.AppendLine($"    - 所在场景: {uiObject.scene.name}");

                // 添加UI对象组件信息
                sb.AppendLine("    - UI对象组件信息:");
                AppendGameObjectComponentInfo(sb, uiObject, "        "); // 增加缩进
                return;
            }

            // 如果没有UI元素，尝试Raycast场景对象
            if (Camera.main != null)
            {
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    var sceneObjectPath = GetGameObjectPath(hit.collider.gameObject);
                    sb.AppendLine($"鼠标下方场景对象路径: {sceneObjectPath}");
                    sb.AppendLine($"    - 所在场景: {hit.collider.gameObject.scene.name}");

                    // 添加场景对象组件信息
                    sb.AppendLine("    - 场景对象组件信息:");
                    AppendGameObjectComponentInfo(sb, hit.collider.gameObject, "        "); // 增加缩进
                    return;
                }
            }
            else
            {
                sb.AppendLine("警告: 场景中没有主摄像机(Camera.main)或未被标记为 'MainCamera'。无法检测鼠标下的场景对象。");
            }

            sb.AppendLine("鼠标位置处没有检测到UI元素或场景对象。");
        }

        /// <summary>
        /// 为指定的GameObject追加其所有组件的信息。
        /// </summary>
        /// <param name="sb">StringBuilder实例。</param>
        /// <param name="obj">目标GameObject。</param>
        /// <param name="indent">当前的缩进字符串。</param>
        private void AppendGameObjectComponentInfo(StringBuilder sb, GameObject obj, string indent)
        {
            Component[] components = obj.GetComponents<Component>();
            if (components.Length == 0)
            {
                sb.AppendLine($"{indent}  - 无组件");
                return;
            }

            foreach (var component in components)
            {
                if (component == null)
                {
                    sb.AppendLine($"{indent}  - (空组件)");
                    continue;
                }

                // 尝试获取Behaviour的enabled状态
                string enabledStatus = (component is Behaviour behaviour) ? behaviour.enabled.ToString() : "N/A";
                sb.AppendLine($"{indent}  - 组件: {component.GetType().Name} (Enabled: {enabledStatus})");
                AppendComponentPropertiesAndFields(sb, component, indent + "    "); // 更深一层缩进，显示组件的属性/字段
            }
        }

        /// <summary>
        /// 利用反射为指定的组件追加其公共属性和字段的信息。
        /// </summary>
        /// <param name="sb">StringBuilder实例。</param>
        /// <param name="component">目标组件。</param>
        /// <param name="indent">当前的缩进字符串。</param>
        private void AppendComponentPropertiesAndFields(StringBuilder sb, Component component, string indent)
        {
            var bindingFlags = BindingFlags.Public | BindingFlags.Instance;
            var componentType = component.GetType();
            // 收集所有公共实例属性和字段
            var members = new List<MemberInfo>();
            members.AddRange(componentType.GetProperties(bindingFlags));
            members.AddRange(componentType.GetFields(bindingFlags));
            bool hasPrintedAnything = false;
            foreach (var member in members)
            {
                // 排除一些常见或特定组件上可能导致冗余或问题的属性/字段
                if (IsMemberToExclude(member, component)) continue;
                object value = null;
                string memberName = member.Name;
                try
                {
                    if (member is PropertyInfo pi)
                    {
                        if (pi.CanRead) // 确保属性可读
                        {
                            value = pi.GetValue(component);
                        }
                        else
                        {
                            // 忽略不可读的属性
                            continue;
                        }
                    }
                    else if (member is FieldInfo fi)
                    {
                        value = fi.GetValue(component);
                    }
                }
                catch (Exception ex)
                {
                    // 捕获获取值时可能发生的异常
                    sb.AppendLine($"{indent}- {memberName}: <获取失败: {ex.GetType().Name}>");
                    hasPrintedAnything = true;
                    continue;
                }

                // 格式化值进行显示，初始深度为0
                string formattedValue = FormatValueForDisplay(value, 0);
                sb.AppendLine($"{indent}- {memberName}: {formattedValue}");
                hasPrintedAnything = true;
            }

            if (!hasPrintedAnything)
            {
                sb.AppendLine($"{indent}- (无公共属性或字段)");
            }
        }

        /// <summary>
        /// 判断一个成员是否应该被排除在打印列表之外。
        /// 用于过滤掉冗余或可能导致深度递归的成员。
        /// </summary>
        /// <param name="member">要检查的MemberInfo。</param>
        /// <param name="component">该成员所属的组件实例。</param>
        /// <returns>如果应排除则返回true。</returns>
        private bool IsMemberToExclude(MemberInfo member, Component component)
        {
            // 排除从UnityEngine.Object继承的常见属性，这些通常是GameObject级别的元数据，
            // 或者可能导致不必要的递归。
            // 特别是gameObject和transform，它们的类型就是GameObject和Transform，递归它们没有意义，
            // 且其值就是组件所依附的GameObject和Transform，已经通过GetGameObjectPath显示了。
            switch (member.Name)
            {
                case "hideFlags": // Unity内部的标志，通常不需要显示
                case "name": // GameObject的名称，已在路径中显示
                case "tag": // GameObject的标签，可从GameObject直接获取
                case "layer": // GameObject的层，可从GameObject直接获取
                case "useGUILayout": // Unity内部GUI相关的，通常不作为组件值关心
                case "runInEditMode": // Unity编辑器模式相关，通常不作为组件值关心
                // 对于Component基类上的gameObject和transform属性，它们直接指向宿主对象和其Transform。
                // 打印它们本身就是重复的且可能误导（不是组件内部的独特“值”）。
                case "gameObject":
                case "transform":
                case "isStatic": // GameObject的isStatic状态
                    return true;
            }

            // 进一步过滤掉一些Unity内部或编辑器相关的属性，这些属性通常在运行时不提供有用的组件值信息。
            if (member.DeclaringType == typeof(Behaviour) || member.DeclaringType == typeof(MonoBehaviour))
            {
                switch (member.Name)
                {
                    case "isActiveAndEnabled": // 行为体的激活状态，通常与enabled一起考虑
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 格式化对象值以进行显示，特别是针对Unity的常见类型，以避免循环打印和提供简洁输出。
        /// </summary>
        /// <param name="value">要格式化的值。</param>
        /// <param name="currentDepth">当前的递归深度。</param>
        /// <returns>格式化后的字符串。</returns>
        private string FormatValueForDisplay(object value, int currentDepth = 0)
        {
            if (value == null) return "null";
            Type type = value.GetType();
            // 深度限制检查：如果超过最大深度，则返回提示信息
            if (currentDepth >= MAX_REFLECTION_DEPTH)
            {
                // 对于集合，提供更具体一些的信息
                if (value is Array array1) return $"Array (Count: {array1.Length}, Max Depth Reached)";
                if (value is IList list1) return $"List (Count: {list1.Count}, Max Depth Reached)";
                if (value is IDictionary dictionary) return $"Dictionary (Count: {dictionary.Count}, Max Depth Reached)";
                return $"[{type.Name} (Max Depth Reached)]";
            }

            // 1. 基本类型、字符串、枚举：直接ToString()
            if (type.IsPrimitive || type == typeof(string) || type.IsEnum)
            {
                return value.ToString();
            }

            // 2. 常见的Unity结构体：特殊格式化，避免深度递归并提供简洁输出
            if (value is Vector2 vec2) return $"({vec2.x:F2}, {vec2.y:F2})";
            if (value is Vector3 vec3) return $"({vec3.x:F2}, {vec3.y:F2}, {vec3.z:F2})";
            if (value is Vector4 vec4) return $"({vec4.x:F2}, {vec4.y:F2}, {vec4.z:F2}, {vec4.w:F2})";

            // Quaternion默认ToString()会显示x,y,z,w，但有时EulerAngles更直观。
            if (value is Quaternion q)
                return
                    $"Q({q.x:F2}, {q.y:F2}, {q.z:F2}, {q.w:F2}) (Euler: {q.eulerAngles.x:F2}, {q.eulerAngles.y:F2}, {q.eulerAngles.z:F2})";

            if (value is Color color) return $"RGBA({color.r:F2}, {color.g:F2}, {color.b:F2}, {color.a:F2})";
            if (value is Rect rect)
                return $"Rect(Pos:({rect.xMin:F2},{rect.yMin:F2}) Size:({rect.width:F2},{rect.height:F2}))";
            if (value is Bounds bounds)
                return
                    $"Bounds(Center:({bounds.center.x:F2},{bounds.center.y:F2},{bounds.center.z:F2}) Extents:({bounds.extents.x:F2},{bounds.extents.y:F2},{bounds.extents.z:F2}))";
            if (value is LayerMask layerMask)
            {
                // LayerMask的值可能代表多个层，或一个单一层。LayerToName只能转换单一层。
                // 对于多个层，返回其原始值更有意义。
                return $"LayerMask(Value: {layerMask.value})";
            }

            // 3. GameObject/Component引用：只打印名称或类型，避免无限递归
            if (value is GameObject go) return $"GameObject:'{go.name}'";
            if (value is Component comp)
                return $"Component:'{comp.GetType().Name}' on GameObject:'{comp.gameObject.name}'";

            // 4. 集合类型：现在会打印内容，调用专门的辅助方法
            if (value is Array array)
            {
                return FormatCollectionForDisplay(array, currentDepth + 1);
            }

            if (value is IList list)
            {
                return FormatCollectionForDisplay(list, currentDepth + 1);
            }

            if (value is IDictionary dict)
            {
                return FormatCollectionForDisplay(dict, currentDepth + 1);
            }

            // 对于其他不可转换为Array/IList/IDictionary的IEnumerable，但又不是字符串的类型
            if (value is IEnumerable enumerable && !(value is string))
            {
                return FormatCollectionForDisplay(enumerable, currentDepth + 1);
            }

            // 5. 其他复杂引用类型：只打印其类型名称，默认不深入其内部 (除非深度允许，但在深度限制前这里只会显示类型名)
            return type.Name; // 例如: Material, Texture2D等，只显示类型名
        }

        /// <summary>
        /// 格式化集合对象以进行显示，支持深度限制和元素数量限制。
        /// </summary>
        /// <param name="collection">要格式化的集合。</param>
        /// <param name="currentDepth">当前的递归深度。</param>
        /// <returns>格式化后的集合字符串。</returns>
        private string FormatCollectionForDisplay(IEnumerable collection, int currentDepth)
        {
            if (collection == null) return "null collection";
            Type collectionType = collection.GetType();
            StringBuilder sb = new StringBuilder();
            // 尝试获取集合的类型名，去除可能的反引号（用于泛型类型）
            string collectionTypeName = collectionType.IsGenericType
                ? collectionType.Name.Substring(0, collectionType.Name.IndexOf('`'))
                : collectionType.Name.Replace("[]", "");
            sb.Append(collectionTypeName);

            sb.Append(" [");
            int i = 0;
            foreach (var item in collection)
            {
                if (i >= MAX_COLLECTION_ELEMENTS_TO_PRINT)
                {
                    sb.Append(", ...");
                    break;
                }

                if (i > 0) sb.Append(", ");
                if (item is DictionaryEntry entry) // 针对非泛型IDictionary
                {
                    sb.Append(
                        $"{{{FormatValueForDisplay(entry.Key, currentDepth + 1)}: {FormatValueForDisplay(entry.Value, currentDepth + 1)}}}");
                }
                else
                {
                    Type itemType = item?.GetType();
                    // 针对泛型IDictionary，其元素是KeyValuePair<TKey, TValue>
                    if (itemType != null && itemType.IsGenericType &&
                        itemType.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
                    {
                        // 使用反射获取Key和Value属性
                        PropertyInfo keyProperty = itemType.GetProperty("Key");
                        PropertyInfo valueProperty = itemType.GetProperty("Value");
                        if (keyProperty != null && valueProperty != null)
                        {
                            object key = keyProperty.GetValue(item);
                            object value = valueProperty.GetValue(item);
                            sb.Append(
                                $"{{{FormatValueForDisplay(key, currentDepth + 1)}: {FormatValueForDisplay(value, currentDepth + 1)}}}");
                        }
                        else
                        {
                            // Fallback in case Key/Value properties aren't found
                            sb.Append(FormatValueForDisplay(item, currentDepth + 1));
                        }
                    }
                    else
                    {
                        // 格式化普通集合元素
                        sb.Append(FormatValueForDisplay(item, currentDepth + 1));
                    }
                }

                i++;
            }

            // 尝试获取集合的实际数量
            string countInfo = "N/A";
            if (collection is ICollection c)
            {
                countInfo = c.Count.ToString();
            }
            else if (collection is Array a)
            {
                countInfo = a.Length.ToString();
            }

            sb.Append($"] (Count: {countInfo})");
            return sb.ToString();
        }

        /// <summary>
        ///     获取给定游戏对象的完整层次路径。
        /// </summary>
        /// <param name="go">要获取路径的游戏对象。</param>
        /// <returns>游戏对象的完整路径，例如 "Parent/Child/Object"。</returns>
        private string GetGameObjectPath(GameObject go)
        {
            if (go == null) return "N/A";

            var path = go.name;
            var currentTransform = go.transform;

            while (currentTransform.parent != null)
            {
                currentTransform = currentTransform.parent;
                path = currentTransform.name + "/" + path;
            }

            return path;
        }
        
    }
}