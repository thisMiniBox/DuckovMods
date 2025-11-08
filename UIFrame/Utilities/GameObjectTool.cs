using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UIFrame.Utilities
{
    public class GameObjectTool
    {
        /// <summary>
        /// 根据Unity对象路径查找场景中的GameObject，包括隐藏（非激活）对象。
        /// 路径示例："RootObject/ChildObject/GrandchildObject"
        /// </summary>
        /// <param name="path">要查找的GameObject的层级路径。</param>
        /// <returns>找到的GameObject，如果未找到则返回null。</returns>
        public static GameObject FindObjectByPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                Debug.LogWarning("FindObjectByPath: Provided path is null, empty, or whitespace. Returning null.");
                return null;
            }
            
            string[] pathParts = path.Split(new char[] { '/' }, System.StringSplitOptions.RemoveEmptyEntries);
            // 如果路径分割后没有有效部分（例如："/" 或空字符串），则直接返回null
            if (pathParts.Length == 0)
            {
                Debug.LogWarning(
                    $"FindObjectByPath: Path '{path}' resulted in no valid segments after splitting. Returning null.");
                return null;
            }

            GameObject currentObject = null;
            // GetRootGameObjects() 会返回场景中所有根级别的GameObject，无论它们是否激活。
            Scene activeScene = SceneManager.GetActiveScene();
            GameObject[] rootGameObjects = activeScene.GetRootGameObjects();
            foreach (GameObject rootObj in rootGameObjects)
            {
                if (rootObj.name == pathParts[0])
                {
                    currentObject = rootObj;
                    break;
                }
            }

            // 如果根对象未找到，则路径无效
            if (currentObject == null)
            {
                Debug.LogWarning($"FindObjectByPath: Root object '{pathParts[0]}' not found in the active scene. Returning null.");
                return null;
            }
            
            for (int i = 1; i < pathParts.Length; i++)
            {
                if (currentObject == null)
                {
                    Debug.LogError(
                        $"FindObjectByPath: Unexpected null currentObject while traversing path segment '{pathParts[i]}'. This indicates an internal logic error.");
                    return null;
                }

                // transform.Find() 能够查找包括非激活状态的子对象
                Transform childTransform = currentObject.transform.Find(pathParts[i]);
                if (childTransform == null)
                {
                    Debug.LogWarning($"FindObjectByPath: Child object '{pathParts[i]}' not found under '{currentObject.name}'. Path invalid. Returning null.");
                    return null;
                }

                currentObject = childTransform.gameObject;
            }
            
            return currentObject;
        }
    }
}