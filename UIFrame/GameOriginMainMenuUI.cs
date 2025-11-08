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
        // public SpriteRenderer? title;
        // public TMP_Text[]? allTexts;
        public static Sprite? titleSprite=null;


        // public void Initialize()
        // {
        //     SceneManager.sceneLoaded += OnSceneLoaded;
        //     // SceneLoader.onAfterSceneInitialize += OnAfterSceneInitialize;
        //     LinkMainMenuObj();
        // }
        //
        // public void Cleanup()
        // {
        //     SceneManager.sceneLoaded -= OnSceneLoaded;
        //     // SceneLoader.onAfterSceneInitialize -= OnAfterSceneInitialize;
        // }
        //
        // private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        // {
        //     Debug.Log("Loading game origin main menu...");
        //     LinkMainMenuObj();
        // }
        // // private void OnAfterSceneInitialize(SceneLoadingContext sceneLoadingContext)
        // // {
        // //     
        // // }
        //
        // public void LinkMainMenuObj()
        // {
        //     var logoObj=GameObjectTool.FindObjectByPath("TimelineContent/LOGO/Logo");
        //     title = logoObj?.GetComponent<SpriteRenderer>();
        // }

        // public bool SetFont(TMP_FontAsset font)
        // {
        //     if(allTexts == null || allTexts.Length == 0)
        //         return false;
        //     foreach (var text in allTexts)
        //     {
        //         text.font = font;
        //     }
        //     return true;
        // }

        public bool SetTitle(Sprite texture)
        {
            // Debug.Log("Setting title...");
            titleSprite = texture;
            // if(title==null)
            // {
            //     return false;
            // }
            // title.sprite = texture;
            return true;
        }
    }
}