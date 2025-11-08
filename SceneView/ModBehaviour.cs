using System;
using HarmonyLib;
using UnityEngine;

namespace SceneView
{
    public class ModBehaviour:Duckov.Modding.ModBehaviour
    {
        public const string MOD_ID="SceneView";
        private Harmony? harmony;
        
        private GameObject? component;
        private CanvasControl myCanvas;
        
        protected override void OnAfterSetup()
        {
            CreateComponents();
            if (harmony == null)
            {
                harmony=new Harmony(MOD_ID);
            }
            harmony.PatchAll();
        }

        protected override void OnBeforeDeactivate()
        {
            RemoveComponents();
            if (harmony != null)
            {
                harmony.UnpatchAll(harmony.Id);
            }

            harmony = null;
        }

        private void CreateComponents()
        {
            if(component==null)
            {
                component = new GameObject("SceneViewControl");
                myCanvas= component.AddComponent<CanvasControl>();
                component.SetActive(true);
                DontDestroyOnLoad(component);
            }
        }

        private void RemoveComponents()
        {
            if (component != null)
            {
                GameObject.Destroy(component);
                component = null;
            }
        }
    }
}