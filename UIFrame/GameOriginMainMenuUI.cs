using Duckov.UI;
using Duckov.Utilities;
using TMPro;
using UIFrame.Utilities;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UIFrame
{
    public class GameOriginMainMenuUI
    {
        public GameObject mainMenuContainer;
        public Image? title;
        public TMP_Text[]? allTexts;
        
        public Sprite titleSprite;
        
        public bool linkMainMenu=false;

        public void Initialize()
        {
            SceneLoader.onAfterSceneInitialize += OnAfterSceneInitialize;
        }

        public void Cleanup()
        {
            SceneLoader.onAfterSceneInitialize -= OnAfterSceneInitialize;
        }

        private void OnAfterSceneInitialize(SceneLoadingContext sceneLoadingContext)
        {
            linkMainMenu = false;
            LinkMainMenuObj();
        }
        
        public void LinkMainMenuObj()
        {
            mainMenuContainer = GameObject.Find("MainMenuContainer");
            if(!mainMenuContainer)
            {
                Debug.LogWarning("Could not find Main Menu Container");
                return;
            }
            Debug.Log("Main Menu Container initialized");
            allTexts = mainMenuContainer.GetComponentsInChildren<TMP_Text>();
            title = GameObjectTool.FindChildByName(mainMenuContainer.transform, "MainTitle")?.GetComponent<Image>();
            linkMainMenu = true;
            
            
        }

        public bool SetFont(TMP_FontAsset font)
        {
            if(allTexts == null || allTexts.Length == 0)
                return false;
            foreach (var text in allTexts)
            {
                text.font = font;
            }
            return true;
        }

        public bool SetTitle(Sprite texture)
        {
            titleSprite=texture;
            if(title==null)
                return false;
            title.sprite = texture;
            return true;
        }
    }
}