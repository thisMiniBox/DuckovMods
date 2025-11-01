
using UnityEngine;
using Object = UnityEngine.Object; // 确保引入 UnityEngine 命名空间

namespace HideCharacter
{
    public class ModBehaviour : Duckov.Modding.ModBehaviour 
    {
        private GameObject? _hideCharacterManagerGameObject=null;
        private const string CHILD_GAMEOBJECT_NAME = "HideCharacterManager";

        protected override void OnAfterSetup()
        {
            AddHideComponent();
        }

        protected override void OnBeforeDeactivate()
        {
            RemoveHideComponent();
            
        }

        private void AddHideComponent()
        {
            var childTransform = this.transform.Find(CHILD_GAMEOBJECT_NAME);
            if (childTransform) return;
            _hideCharacterManagerGameObject = new GameObject(CHILD_GAMEOBJECT_NAME);
            _hideCharacterManagerGameObject.transform.SetParent(this.transform);
            _hideCharacterManagerGameObject.AddComponent<HideCharacterComponent>();
        }

        private void RemoveHideComponent()
        {
            if (_hideCharacterManagerGameObject)
                Destroy(_hideCharacterManagerGameObject);
        }
    }
}


