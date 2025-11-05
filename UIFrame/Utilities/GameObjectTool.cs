using System.Collections.Generic;
using UnityEngine;

namespace UIFrame.Utilities
{
    public class GameObjectTool
    {
        /// <summary>
        /// 在指定父对象下，查找第一个匹配名称的子GameObject（可以是孙子、曾孙等）
        /// </summary>
        /// <param name="parent">要查找的父Transform。</param>
        /// <param name="name">要查找的GameObject的名称。</param>
        /// <returns>找到的GameObject，如果未找到则返回null。</returns>
        public static GameObject? FindChildByName(Transform parent, string name)
        {
            // 查找父对象本身是否就是目标对象
            if (parent.name.Equals(name))
            {
                return parent.gameObject;
            }
            // 遍历所有直接子对象
            foreach (Transform child in parent)
            {
                // 检查当前子对象是否是目标对象
                if (child.name.Equals(name))
                {
                    return child.gameObject;
                }
                // 递归查找子对象的子对象
                var found = FindChildByName(child, name);
                if (found)
                {
                    return found;
                }
            }
            return null;
        }
        /// <summary>
        /// 在指定父对象下，查找所有匹配名称的子GameObject（可以是孙子、曾孙等），不区分大小写。
        /// </summary>
        /// <param name="parent">要查找的父Transform。</param>
        /// <param name="name">要查找的GameObject的名称。</param>
        /// <returns>所有找到的GameObject列表。</returns>
        public static List<GameObject> FindChildrenByName(Transform parent, string name)
        {
            var foundObjects = new List<GameObject>();
            FindChildrenByNameRecursive(parent, name, foundObjects);
            return foundObjects;
        }
        private static void FindChildrenByNameRecursive(Transform currentTransform, string name, List<GameObject> foundObjects)
        {
            // 检查当前对象是否是目标对象
            if (currentTransform.name.Equals(name))
            {
                foundObjects.Add(currentTransform.gameObject);
            }
            // 遍历所有子对象并递归查找
            foreach (Transform child in currentTransform)
            {
                FindChildrenByNameRecursive(child, name, foundObjects);
            }
        }
        
        
    }
}