using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Duckov;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace HitFeedback
{
    public class ModBehaviour:Duckov.Modding.ModBehaviour
    {
        public const string AudioFolderName = "audio";
        public string audioFolderPath;

        public List<string> audioFilePath = new List<string>();
        
        public Health health;
        
        public Config config=new Config();

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F8))
            {
                PlayRandomAudioClip();
            }
        }

        protected override void OnAfterSetup()
        {
            LevelManager.OnLevelInitialized += OnSceneLoaded;
            audioFolderPath=Path.Combine(info.path,AudioFolderName);
            FindWavFiles();
        }
        protected override void OnBeforeDeactivate()
        {
            
            LevelManager.OnLevelInitialized -= OnSceneLoaded;
        }
        private void FindWavFiles()
        {
            audioFilePath.Clear();
            if (!Directory.Exists(audioFolderPath))
            {
                return;
            }
            try
            {
                string[] files = Directory.GetFiles(audioFolderPath, "*.wav", SearchOption.TopDirectoryOnly);
                if (files.Length > 0)
                {
                    foreach (string filePath in files)
                    {
                        audioFilePath.Add(filePath);
                    }
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                Debug.LogError($"Error: Access to '{audioFolderPath}' is denied. {ex.Message}");
            }
            catch (DirectoryNotFoundException ex)
            {
                Debug.LogError($"Error: Directory '{audioFolderPath}' not found. {ex.Message}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"An unexpected error occurred: {ex.Message}");
            }
        }

        private void OnSceneLoaded()
        {
            TryAddListener();
        }

        private void TryAddListener()
        {
            if (health)
            {
                health.OnHurtEvent.RemoveListener(OnHurtEvent);
            }
            health = CharacterMainControl.Main?.Health;
            if (health)
            {
                health.OnHurtEvent.AddListener(OnHurtEvent);
            }

        }

        private void OnHurtEvent(DamageInfo damageInfo)
        {
            PlayRandomAudioClip();
        }
        
        public void PlayRandomAudioClip()
        {
            if (audioFilePath.Count > 0)
            {
                var randomIndex = Random.Range(0, audioFilePath.Count);
                var filePath = audioFilePath[randomIndex];
                AudioManager.PostCustomSFX(filePath);
            }
            else
            {
                Debug.LogWarning("Mod Feedback: No audio clips loaded to play.");
            }
        }
        
        
    }
}