using System.Reflection;
using UnityEngine;

namespace SceneSnapshot
{
    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {
        protected override void OnAfterSetup()
        {
            AddPrintToolToScene();
        }

        protected override void OnBeforeDeactivate()
        {
            RemovePrintToolFromScene();
        }

        /// <summary>
        /// 检查场景中是否已存在PrintTool，如果不存在则添加一个新的。
        /// </summary>
        private void AddPrintToolToScene()
        {
            if (GameObject.FindObjectOfType<PrintTool>() == null)
            {
                var printToolGO = new GameObject("PrintTool_Monitor");
                printToolGO.transform.SetParent(this.transform);
                printToolGO.AddComponent<PrintTool>();
            }
        }
        private void RemovePrintToolFromScene()
        {
            var printTool = GameObject.FindObjectOfType<PrintTool>();
            if (printTool != null)
            {
                GameObject.Destroy(printTool.gameObject);
            }
        }
    }
}
