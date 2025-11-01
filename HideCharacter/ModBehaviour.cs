
using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object; // 确保引入 UnityEngine 命名空间

namespace HideCharacter
{
    public class ModBehaviour : Duckov.Modding.ModBehaviour 
    {
        public static HideCharacterComponent? hideHideCharacterManager=null;
        private const string CHILD_GAMEOBJECT_NAME = "HideCharacterManager";

        public string MOD_ID = "HideCharacter";
        private Harmony _harmony;
        
        protected override void OnAfterSetup()
        {
            AddHideComponent();
            if (_harmony == null)
            {
                _harmony=new Harmony(MOD_ID);
                _harmony.PatchAll();
            }
        }

        protected override void OnBeforeDeactivate()
        {
            RemoveHideComponent();
            if (_harmony != null)
            {
                _harmony.UnpatchAll(MOD_ID);
                _harmony = null;
            }
        }

        private void AddHideComponent()
        {
            var childTransform = this.transform.Find(CHILD_GAMEOBJECT_NAME);
            if (childTransform) return;
            
            var hideCharacterManagerGameObject = new GameObject(CHILD_GAMEOBJECT_NAME);
            hideCharacterManagerGameObject.transform.SetParent(this.transform);
            hideHideCharacterManager = hideCharacterManagerGameObject.AddComponent<HideCharacterComponent>();
        }

        private void RemoveHideComponent()
        {
            if (hideHideCharacterManager)
                Destroy(hideHideCharacterManager?.gameObject);
        }
    }
}


