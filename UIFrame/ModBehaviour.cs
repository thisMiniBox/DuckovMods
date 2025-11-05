using System;
using Duckov.Modding;
using Duckov.Options.UI;
using Duckov.UI;
using Duckov.UI.Animations;
using Duckov.Utilities;
using HarmonyLib;
using UnityEngine;

namespace UIFrame
{
    public class ModBehaviour:Duckov.Modding.ModBehaviour
    {
        private const string MOD_ID="UIFrame";
        
        private GameObject? workerObject;
        
        private Harmony? harmony;
        
        protected override void OnAfterSetup()
        {
            CreateAPIObject();
            if (harmony == null)
            {
                harmony=new HarmonyLib.Harmony(MOD_ID);
            }
            harmony.PatchAll();
            Test();
        }
        

        protected override void OnBeforeDeactivate()
        {
            ClearAPIObject();
            harmony?.UnpatchAll(MOD_ID);
            harmony = null;
        }

        private void CreateAPIObject()
        {
            if(workerObject)
                return;
            workerObject = new GameObject($"{MOD_ID}_APIObject");
            workerObject.AddComponent<UIFrameWorker>();
        }

        private void ClearAPIObject()
        {
            if(!workerObject)
                return;
            Destroy(workerObject);
            workerObject = null;
        }

        private void Test()
        {
            if(!UIFrameAPI.Initialize())
                return;
            if (UIFrameAPI.SetGameTitle(@"C:\Users\Lenovo\Pictures\异噬.png"))
            {
                Debug.Log("设置标题完成");
            }
            else
            {
                Debug.Log("标题设置失败");
            }
        }
    }
}