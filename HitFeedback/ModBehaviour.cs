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
        public const string ConfigFileName = "config.ini";
        public string audioFolderPath;
        public string configFilePath;

        public Dictionary<string,float> audioFilePath = new Dictionary<string,float>();
        
        public Health health;
        
        public Config config=new Config();
        
        public float totalWeight;

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
            configFilePath=Path.Combine(info.path, ConfigFileName);
            FindWavFiles();
            config.LoadConfig(configFilePath);
            ApplyConfig();
            foreach (var f in audioFilePath)
            {
                totalWeight+=f.Value;
            }
        }
        protected override void OnBeforeDeactivate()
        {
            LevelManager.OnLevelInitialized -= OnSceneLoaded;
            SaveConfig();
        }

        private void OnDestroy()
        {
            SaveConfig();
        }

        private void ApplyConfig()
        {
            foreach (var f in config.probability)
            {
                if (audioFilePath.ContainsKey(f.Key))
                {
                    audioFilePath[f.Key] = f.Value;
                }
            }
        }

        private void SaveConfig()
        {
            config.probability.Clear();
            foreach (var f in audioFilePath)
            {
                config.probability.Add(f.Key, f.Value);
            }

            config.SaveConfig(configFilePath);
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
                        audioFilePath.Add(Path.GetFileName(filePath), 1);
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
                var randomIndex = Random.Range(0, totalWeight);
                foreach (var f in audioFilePath)
                {
                    randomIndex -= f.Value;
                    if (randomIndex <= 0)
                    {
                        AudioManager.PostCustomSFX(Path.Combine(audioFolderPath, f.Key));
                        return;
                    }
                }
            }
            else
            {
                Debug.LogWarning("Mod Feedback: No audio clips loaded to play.");
            }
        }
    }
}