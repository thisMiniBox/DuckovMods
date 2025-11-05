using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems; // 用于UI射线检测
using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Collections; // 用于 IEnumerable
using System.Collections.Generic; // 用于 HashSet, List, Dictionary
using System.Linq; // 用于 OrderBy

namespace SceneSnapshot
{
    internal class PrintTool : MonoBehaviour
    {
        private const string BASE_FOLDER_NAME = "GameObjectSnapshots"; // 主文件夹名称
        private const int MAX_REFLECTION_DEPTH = 3; // 最大反射深度，防止循环引用或过深的对象图
        private const int MAX_COLLECTION_ELEMENTS_TO_PRINT = 5; // 集合最多打印的元素数量

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F2))
            {
                CaptureAndPrintSnapshot();
            }
        }

        private void CaptureAndPrintSnapshot()
        {
            // 获取桌面路径
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            // 1. 创建主文件夹 (如果不存在)
            string baseOutputPath = Path.Combine(desktopPath, BASE_FOLDER_NAME);
            try
            {
                if (!Directory.Exists(baseOutputPath))
                {
                    Directory.CreateDirectory(baseOutputPath);
                    Debug.Log($"已创建主快照文件夹: {baseOutputPath}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"无法创建主快照文件夹 {baseOutputPath}: {e.Message}");
                return;
            }

            // 2. 在主文件夹内创建带时间戳的子文件夹
            string timestampFolderName = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string currentSnapshotOutputPath = Path.Combine(baseOutputPath, timestampFolderName);

            try
            {
                if (!Directory.Exists(currentSnapshotOutputPath))
                {
                    Directory.CreateDirectory(currentSnapshotOutputPath);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"无法创建快照子文件夹 {currentSnapshotOutputPath}: {e.Message}");
                return;
            }

            Debug.Log($"开始生成场景快照到: {currentSnapshotOutputPath}");

            // Part 1: 打印所有对象的对象树及其组件
            PrintAllGameObjectsTree(currentSnapshotOutputPath);

            // Part 2: 打印鼠标位置对象的组件值
            PrintMouseHoveredObjectDetails(currentSnapshotOutputPath);

            Debug.Log("场景快照生成完毕!");
        }

        /// <summary>
        /// 打印所有场景对象的对象树（包括DontDestroyOnLoad）及其组件。
        /// </summary>
        private void PrintAllGameObjectsTree(string outputPath)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("--- 所有激活场景对象树 ---");
            sb.AppendLine("--------------------------\n");

            // 用于存储按场景分组的根对象
            var sceneRootGameObjects = new Dictionary<Scene, List<GameObject>>();
            // 用于存储 DontDestroyOnLoad 对象
            var dontDestroyOnLoadRoots = new List<GameObject>();

            // 1. 遍历所有加载的场景，获取其根对象
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                sceneRootGameObjects[scene] = new List<GameObject>(scene.GetRootGameObjects());
            }

            // 2. 查找 DontDestroyOnLoad 对象
            // DontDestroyOnLoad 对象不属于任何通过 SceneManager.GetSceneAt 获取的“普通”场景
            // 它们通常在特殊的 "DontDestroyOnLoad" 场景中（在Unity编辑器中可见），但在运行时无法直接通过 SceneManager.GetSceneAt 访问。
            // 因此，我们遍历所有活跃的GameObject，找出那些是根对象但又不属于任何已知场景的。
            GameObject[] allActiveGameObjectsInHierarchy = FindObjectsOfType<GameObject>(); // 获取所有活跃的GameObject

            foreach (GameObject go in allActiveGameObjectsInHierarchy)
            {
                if (go.transform.parent == null) // 这是一个根对象
                {
                    bool foundInLoadedScene = false;
                    foreach (var kvp in sceneRootGameObjects)
                    {
                        if (kvp.Value.Contains(go))
                        {
                            foundInLoadedScene = true;
                            break;
                        }
                    }

                    if (!foundInLoadedScene)
                    {
                        // 如果它不是任何已加载场景的根对象，那么它可能是DontDestroyOnLoad对象
                        dontDestroyOnLoadRoots.Add(go);
                    }
                }
            }
            
            // 3. 打印普通场景的对象树
            foreach (var kvp in sceneRootGameObjects)
            {
                Scene currentScene = kvp.Key;
                List<GameObject> roots = kvp.Value;

                sb.AppendLine($"=== 场景: {currentScene.name} (路径: {currentScene.path}, 已加载: {currentScene.isLoaded}) ===\n");

                // 按名称排序根对象以保持输出一致性
                foreach (GameObject root in roots.OrderBy(g => g.name))
                {
                    PrintGameObjectRecursive(root, 0, sb, new HashSet<GameObject>());
                }
            }

            // 4. 打印 DontDestroyOnLoad 对象的对象树
            if (dontDestroyOnLoadRoots.Count > 0)
            {
                    // 检查是否已经有一个伪的 "DontDestroyOnLoad" 场景被 Unity 在某些情境下自动添加
                    // 如果是，为了避免重复，且让输出更清晰，可以先尝试移除这些。
                    // 但是在 FindObjectsOfType 之后再分组，这种方式更健壮，不用管它是否有“场景”
                
                sb.AppendLine("\n=== DontDestroyOnLoad 对象 ===\n");
                foreach (GameObject root in dontDestroyOnLoadRoots.OrderBy(g => g.name))
                {
                    PrintGameObjectRecursive(root, 0, sb, new HashSet<GameObject>());
                }
            }

            string filePath = Path.Combine(outputPath, "SceneObjectTree.txt");
            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8); // 使用UTF8编码以支持更多字符
            Debug.Log($"场景对象树已保存到: {filePath}");
        }

        /// <summary>
        /// 递归打印GameObject及其子级和组件。
        /// </summary>
        private void PrintGameObjectRecursive(GameObject go, int depth, StringBuilder sb, HashSet<GameObject> visited)
        {
            // 防止循环引用或重复打印
            if (visited.Contains(go))
            {
                sb.AppendLine($"{GetIndent(depth)}{go.name} (循环引用检测到!)");
                return;
            }
            visited.Add(go);

            string indent = GetIndent(depth);
            sb.AppendLine($"{indent}GameObject: {go.name} (激活状态: {go.activeSelf}, 标签: {go.tag}, 层: {LayerMask.LayerToName(go.layer)})");

            Component[] components = go.GetComponents<Component>();
            foreach (Component comp in components)
            {
                if (comp == null) continue; // 避免NRE，尽管不常见
                sb.AppendLine($"{indent}  组件: {comp.GetType().Name}");
            }

            for (int i = 0; i < go.transform.childCount; i++)
            {
                PrintGameObjectRecursive(go.transform.GetChild(i).gameObject, depth + 1, sb, visited);
            }
            
            // 重要：在递归完成后通常不需要从visited中移除GameObject，
            // 因为一个GameObject在对象树中只会以唯一的路径出现一次
            // (除非它以某种非常规方式被引用，但这不属于标准GameObject层级)。
            // 对于值类型或简单引用，可以在PrintObjectProperties中在处理完后移除。
            // 对于GameObject层级，一旦访问完其所有子节点，它在该“分支”的任务就完成了。
            // 如果不同根节点下可能会有相同的GameObject引用（例如通过Inspector引用），
            // 那visited集合的作用是防止在*当前递归路径*中再次遇到同一个GameObject，从而避免死循环。
            // 对于整个场景树的打印，visited集合可以保持不变，因为我们不期望同一个GameObject作为不同根节点的子物体链中的一部分。
            // visited.Remove(go); // 对于GameObject树结构，这通常是不必要的，因为每个GameObject在树中只有一个父级。
        }

        /// <summary>
        /// 打印鼠标位置对象的组件值。
        /// </summary>
        private void PrintMouseHoveredObjectDetails(string outputPath)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("--- 鼠标悬停对象详细信息 ---");
            sb.AppendLine("----------------------------\n");

            GameObject hoveredObject = GetHoveredObject();

            if (hoveredObject != null)
            {
                sb.AppendLine($"悬停的GameObject: {hoveredObject.name} (激活状态: {hoveredObject.activeSelf}, 标签: {hoveredObject.tag}, 层: {LayerMask.LayerToName(hoveredObject.layer)})");
                sb.AppendLine($"组件及其值:\n");

                Component[] components = hoveredObject.GetComponents<Component>();
                foreach (Component comp in components)
                {
                    if (comp == null) continue;
                    sb.AppendLine($"  === 组件: {comp.GetType().Name} ===");
                    // 使用反射打印组件的字段和属性值
                    PrintObjectProperties(comp, 0, sb, new HashSet<object>(), "    "); // 初始缩进4个空格
                    sb.AppendLine();
                }
            }
            else
            {
                sb.AppendLine("当前鼠标下方没有对象。");
            }

            string filePath = Path.Combine(outputPath, "MouseHoverObjectDetails.txt");
            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8); // 使用UTF8编码以支持更多字符
            Debug.Log($"鼠标悬停对象详情已保存到: {filePath}");
        }

        /// <summary>
        /// 获取鼠标下方的GameObject（优先UI，其次3D场景对象）。
        /// </summary>
        private GameObject GetHoveredObject()
        {
            // 优先检测UI对象
            if (EventSystem.current != null)
            {
                PointerEventData eventData = new PointerEventData(EventSystem.current);
                eventData.position = Input.mousePosition;
                List<RaycastResult> uiRaycastResults = new List<RaycastResult>();
                EventSystem.current.RaycastAll(eventData, uiRaycastResults);

                // 过滤掉非 interactable 的UI元素或者不包含 CanvasRenderer 的元素，可能更关注可见和可交互的UI
                foreach (var result in uiRaycastResults)
                {
                    if (result.gameObject != null && result.gameObject.GetComponent<CanvasRenderer>() != null)
                    {
                        return result.gameObject; // 返回第一个有效的UI元素
                    }
                }
            }
            else
            {
                Debug.LogWarning("场景中没有EventSystem，无法检测UI对象。请确保场景中存在一个EventSystem GameObject。");
            }

            // 如果没有UI对象，则检测3D场景对象
            if (Camera.main != null)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                // 仅检测默认层，或可配置的层
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    return hit.collider.gameObject;
                }
            }
            else
            {
                Debug.LogWarning("场景中没有主摄像机(Tagged 'MainCamera')，无法进行3D射线检测。请确保主摄像机正确标记。");
            }

            return null;
        }

        /// <summary>
        /// 使用反射递归打印对象的字段和属性值。
        /// </summary>
        /// <param name="obj">要打印的对象。</param>
        /// <param name="currentDepth">当前反射深度。</param>
        /// <param name="sb">StringBuilder用于构建输出。</param>
        /// <param name="visitedObjects">用于检测循环引用的已访问对象集合。</param>
        /// <param name="indent">当前缩进字符串。</param>
        private void PrintObjectProperties(object obj, int currentDepth, StringBuilder sb, HashSet<object> visitedObjects, string indent)
        {
            if (obj == null)
            {
                sb.AppendLine("null");
                return;
            }

            Type type = obj.GetType();

            // 1. 处理基本类型、字符串、枚举
            // 注意：string是引用类型，但行为上通常被视为值类型，其ToString是其自身
            if (type.IsPrimitive || obj is string || type.IsEnum || obj is decimal || obj is DateTime)
            {
                sb.AppendLine($"({type.Name}) {obj}"); // 显示类型名，更清晰
                return;
            }

            // 2. 处理常见的Unity值类型（如Vector3, Quaternion, Color, Rect等）
            // 这些类型通常有很好的ToString()方法，且不应过度深入反射其内部字段
            if (obj is Vector2 || obj is Vector3 || obj is Vector4 || obj is Quaternion ||
                obj is Color || obj is Color32 || obj is Rect || obj is Bounds ||
                obj is AnimationCurve || obj is LayerMask || obj is Matrix4x4)
            {
                sb.AppendLine($"({type.Name}) {obj}"); // 显示类型名，更清晰
                return;
            }
            
            // 3. 处理UnityEngine.Object类型（但不是Component或GameObject本身）
            // 例如 Material, Texture, ScriptableObject等，通常只打印其名称或ToString()就足够
            if (typeof(UnityEngine.Object).IsAssignableFrom(type) && !(obj is Component) && !(obj is GameObject))
            {
                // 对于这些Unity对象，ToString()通常会返回对象名和类型，足够了
                sb.AppendLine($"({type.Name}) {obj.ToString()}");
                return;
            }
            
            // 4. 检查最大反射深度
            if (currentDepth >= MAX_REFLECTION_DEPTH)
            {
                sb.AppendLine($"{indent}... (达到最大反射深度)");
                return;
            }

            // 5. 检查循环引用（仅对引用类型有效，且不是字符串那种特殊引用类型）
            if (!type.IsValueType && !type.IsPrimitive && !(obj is string))
            {
                if (visitedObjects.Contains(obj))
                {
                    sb.AppendLine($"{indent}... (检测到循环引用: {type.Name})");
                    return;
                }
                visitedObjects.Add(obj); // 标记为已访问
            }

            // 6. 处理集合类型（数组、List、Dictionary等，但不包括字符串）
            if (obj is IEnumerable enumerable)
            {
                // 对于字典，直接打印IEnumerable会导致键值对混乱，需要特殊处理
                if (obj is IDictionary dictionary)
                {
                    sb.AppendLine($"({type.Name}) Count={dictionary.Count} {{");
                    int count = 0;
                    foreach (DictionaryEntry entry in dictionary)
                    {
                        if (count >= MAX_COLLECTION_ELEMENTS_TO_PRINT)
                        {
                            sb.AppendLine($"{indent + "  "}...(已截断，显示了{MAX_COLLECTION_ELEMENTS_TO_PRINT}对键值)");
                            break;
                        }
                        sb.Append($"{indent + "  "}[Key]: ");
                        PrintObjectProperties(entry.Key, currentDepth + 1, sb, visitedObjects, indent + "    "); // 额外的缩进
                        sb.Append($"{indent + "  "}[Value]: ");
                        PrintObjectProperties(entry.Value, currentDepth + 1, sb, visitedObjects, indent + "    "); // 额外的缩进
                        count++;
                    }
                    sb.AppendLine($"{indent}}}");
                }
                else // 普通的IEnumerable（List, Array等）
                {
                    int elementCount = 0;
                    if (obj is ICollection collection)
                        elementCount = collection.Count;
                    else if (obj is Array array)
                        elementCount = array.Length;
                    else // 对于无法直接获取Count的IEnumerable，需要遍历统计
                    {
                        var list = new List<object>();
                        foreach (var item in enumerable) list.Add(item);
                        elementCount = list.Count;
                        enumerable = list; // 重新赋值为可以重复遍历的list
                    }

                    sb.AppendLine($"({type.Name}) Count={elementCount} [");
                    int count = 0;
                    foreach (var item in enumerable)
                    {
                        if (count >= MAX_COLLECTION_ELEMENTS_TO_PRINT)
                        {
                            sb.AppendLine($"{indent + "  "}...(已截断，显示了{MAX_COLLECTION_ELEMENTS_TO_PRINT}个元素)");
                            break;
                        }
                        sb.Append($"{indent + "  "}- ");
                        PrintObjectProperties(item, currentDepth + 1, sb, visitedObjects, indent + "  ");
                        count++;
                    }
                    sb.AppendLine($"{indent}]");
                }
                
                // 集合本身通常不直接导致循环引用其自身，其内部元素才可能。
                // 故在处理集合后可以从visitedObjects中移除集合对象，防止它阻止其他路径对它的访问。
                if (!type.IsValueType && !type.IsPrimitive && !(obj is string)) visitedObjects.Remove(obj); 
                return;
            }

            // 7. 处理一般对象（类或结构体）的字段和属性
            sb.AppendLine($"({type.Name}) {{");

            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

            // 字段
            FieldInfo[] fields = type.GetFields(flags);
            foreach (FieldInfo field in fields)
            {
                if (field.IsStatic) continue; // 跳过静态字段
                if (field.IsDefined(typeof(ObsoleteAttribute), true)) continue; // 跳过过时字段

                string propertyIndent = indent + "  ";
                sb.Append($"{propertyIndent}{field.Name}: ");
                try
                {
                    object fieldValue = field.GetValue(obj);
                    PrintObjectProperties(fieldValue, currentDepth + 1, sb, visitedObjects, propertyIndent);
                }
                catch (Exception e)
                {
                    sb.AppendLine($"<无法获取值: {e.Message}>");
                }
            }

            // 属性
            PropertyInfo[] properties = type.GetProperties(flags);
            foreach (PropertyInfo prop in properties)
            {
                // 跳过特殊名称属性（如Unity内部的hideFlags）、不可读属性、带索引器属性、过时属性
                if (prop.IsSpecialName || !prop.CanRead || prop.GetIndexParameters().Length > 0 ||
                    prop.IsDefined(typeof(ObsoleteAttribute), true)) continue;

                string propertyIndent = indent + "  ";
                sb.Append($"{propertyIndent}{prop.Name}: ");
                try
                {
                    object propValue = prop.GetValue(obj);
                    PrintObjectProperties(propValue, currentDepth + 1, sb, visitedObjects, propertyIndent);
                }
                catch (Exception e)
                {
                    sb.AppendLine($"<无法获取值: {e.Message}>");
                }
            }

            sb.AppendLine($"{indent}}}");

            // 在对象处理完毕后，从已访问集合中移除（如果它是引用类型），
            // 这允许在对象图的不同路径中再次遇到它（如果需要），但防止当前路径的循环。
            if (!type.IsValueType && !type.IsPrimitive && !(obj is string)) visitedObjects.Remove(obj);
        }

        /// <summary>
        /// 获取指定深度的缩进字符串。
        /// </summary>
        private string GetIndent(int depth)
        {
            return new string(' ', depth * 2);
        }
    }
}

