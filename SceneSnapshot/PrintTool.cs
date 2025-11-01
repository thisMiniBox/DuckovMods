using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace SceneSnapshot
{
    internal class PrintTool : MonoBehaviour
    {
        private const string FOLDER_NAME = "GameObjectSnapshots";

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
        ///     尝试检测鼠标位置下方的UI元素或场景对象，并将其路径追加到StringBuilder。
        /// </summary>
        /// <param name="sb">StringBuilder实例。</param>
        private void AppendMouseHoverObjectInfo(StringBuilder sb)
        {
            // 首先尝试Raycast UI元素
            var eventDataCurrentPosition = new PointerEventData(EventSystem.current);
            eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            var results = new List<RaycastResult>();

            if (EventSystem.current) EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

            if (results.Count > 0)
            {
                // UI元素优先级更高
                var uiObject = results[0].gameObject;
                var uiPath = GetGameObjectPath(uiObject);
                sb.AppendLine($"鼠标下方UI路径: {uiPath}");
                sb.AppendLine($"    - 所在场景: {uiObject.scene.name}"); // 添加所在场景信息
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
                    sb.AppendLine($"    - 所在场景: {hit.collider.gameObject.scene.name}"); // 添加所在场景信息
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