using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MyMainMenu
{
    public class ModBehaviour:Duckov.Modding.ModBehaviour
    {
        public GameMainTitle gameMainTitle=new GameMainTitle();

        void Awake()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void Update()
        {
            gameMainTitle.Update();
        }

        protected override void OnAfterSetup()
        {
            
        }

        protected override void OnBeforeDeactivate()
        {
            
        }
        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            gameMainTitle.Initialize();
        }
        
    }
}