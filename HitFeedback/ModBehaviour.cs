using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Duckov;
using HitFeedback.Api;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace HitFeedback
{
    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {
        public const string AudioFolderName = "audio";
        public const string ConfigFileName = "config.ini";
        public string audioFolderPath;
        public string configFilePath;

        public Dictionary<string, float> audioProbability = new Dictionary<string, float>();

        public Health health;

        public Config config = new Config();

        public float totalWeight;

        public const string MOD_SETTING_NAME = "受击反馈";

        private void Update()
        {
            if (Input.GetKeyDown(config.hotKey))
            {
                PlayRandomAudioClip();
            }
        }

        protected override void OnAfterSetup()
        {
            LevelManager.OnLevelInitialized += OnSceneLoaded;
            audioFolderPath = Path.Combine(info.path, AudioFolderName);
            configFilePath = Path.Combine(info.path, ConfigFileName);
            FindWavFiles();
            config.LoadConfig(configFilePath);
            ApplyConfig();

            InitializeSetting();
            UpdateTotalWeight();
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

        private void UpdateTotalWeight()
        {
            totalWeight = 0;
            foreach (var f in audioProbability)
            {
                totalWeight += f.Value;
            }
        }

        private void ApplyConfig()
        {
            foreach (var f in config.probability)
            {
                if (audioProbability.ContainsKey(f.Key))
                {
                    audioProbability[f.Key] = f.Value;
                }
            }
        }

        private void SaveConfig()
        {
            config.probability.Clear();
            foreach (var f in audioProbability)
            {
                config.probability.Add(f.Key, f.Value);
            }

            config.SaveConfig(configFilePath);
        }

        private void FindWavFiles()
        {
            audioProbability.Clear();
            if (!Directory.Exists(audioFolderPath))
            {
                return;
            }

            try
            {
                var audioFiles = new List<string>();
                string[] wavFiles = Directory.GetFiles(audioFolderPath, "*.wav", SearchOption.TopDirectoryOnly);
                audioFiles.AddRange(wavFiles);
                string[] mp3Files = Directory.GetFiles(audioFolderPath, "*.mp3", SearchOption.TopDirectoryOnly);
                audioFiles.AddRange(mp3Files);
                string[] oggFiles = Directory.GetFiles(audioFolderPath, "*.ogg", SearchOption.TopDirectoryOnly);
                audioFiles.AddRange(oggFiles);
                if (audioFiles.Count > 0)
                {
                    foreach (var filePath in audioFiles)
                    {
                        audioProbability.Add(Path.GetFileName(filePath), 1);
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
            if (config.ShouldPlayAudioFeedback(damageInfo))
                PlayRandomAudioClip();
        }

        public void PlayRandomAudioClip()
        {
            if (audioProbability.Count > 0)
            {
                var randomIndex = Random.Range(0, totalWeight);
                foreach (var f in audioProbability)
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

        public void InitializeSetting()
        {
            if (!Api.ModConfigAPI.Initialize())
            {
                return;
            }
            

            foreach (var audio in audioProbability)
            {
                ModConfigAPI.SafeAddInputWithSlider(MOD_SETTING_NAME, audio.Key, $"音频\"{audio.Key}\"播放概率",
                    typeof(float), audio.Value, new Vector2(0, 100));
            }

            foreach (DamageFeature value in Enum.GetValues(typeof(DamageFeature)))
            {
                ModConfigAPI.SafeAddBoolDropdownList(MOD_SETTING_NAME, value.ToString(),
                    $"受到{ToSingleFeatureChineseString(value)}时触发",
                    config.audioDamageFeatures.Contains(value));
            }

            var hotkeyOptions = GenerateCommonKeyCodeOptions();
            ModConfigAPI.SafeAddDropdownList(
                MOD_SETTING_NAME,
                "hotKey",
                "主动触发的热键",
                hotkeyOptions,
                typeof(int),
                config.hotKey
            );


            ModConfigAPI.SafeAddOnOptionsChangedDelegate(OnConfigChange);
        }



        private void OnConfigChange(string key)
        {
            key = key[(MOD_SETTING_NAME.Length + 1)..];
            if (key == "hotKey")
            {
                config.hotKey = (KeyCode)ModConfigAPI.SafeLoad(MOD_SETTING_NAME, key, (int)(config.hotKey));
                return;
            }
            if (audioProbability.ContainsKey(key))
            {
                var value = ModConfigAPI.SafeLoad(MOD_SETTING_NAME, key, audioProbability[key]);
                audioProbability[key] = value;
            }

            if (Enum.TryParse(key, out DamageFeature damageInfo))
            {
                var current=config.audioDamageFeatures.Contains(damageInfo);
                if (ModConfigAPI.SafeLoad(MOD_SETTING_NAME, key, current))
                {
                    config.audioDamageFeatures.Add(damageInfo);
                }
                else if (current)
                {
                    config.audioDamageFeatures.Remove(damageInfo);
                }
            }

            UpdateTotalWeight();

        } 
        
        /// <summary>
        /// 生成包含常用 KeyCode 的 SortedDictionary。
        /// </summary>
        /// <returns>一个 SortedDictionary，键是 KeyCode 的字符串表示，值是 KeyCode 枚举本身。</returns>
        private static SortedDictionary<string, object> GenerateCommonKeyCodeOptions()
        {
            var options = new SortedDictionary<string, object>();
            // 字母键
            for (var c = 'A'; c <= 'Z'; c++)
            {
                var keyCode = (int)(KeyCode)Enum.Parse(typeof(KeyCode), c.ToString());
                options.Add(c.ToString(), keyCode);
            }

            // 数字键（主键盘）
            for (var i = 0; i <= 9; i++)
            {
                var keyCode = (int)(KeyCode)Enum.Parse(typeof(KeyCode), "Alpha" + i.ToString());
                options.Add(i.ToString(), keyCode);
            }

            // 数字键盘
            for (var i = 0; i <= 9; i++)
            {
                var keyCode = (int)(KeyCode)Enum.Parse(typeof(KeyCode), "Keypad" + i.ToString());
                options.Add($"Num_{i}", keyCode); // 加前缀区分主键盘数字
            }

            // 功能键
            for (var i = 1; i <= 12; i++)
            {
                var keyCode = (int)(KeyCode)Enum.Parse(typeof(KeyCode), "F" + i.ToString());
                options.Add($"F{i}", keyCode);
            }

            // 常用控制键
            options.Add("空格", (int)KeyCode.Space);
            options.Add("回车", (int)KeyCode.Return);
            options.Add("Esc", (int)KeyCode.Escape);
            options.Add("Shift (左)", (int)KeyCode.LeftShift);
            options.Add("Shift (右)", (int)KeyCode.RightShift);
            options.Add("Ctrl (左)", (int)KeyCode.LeftControl);
            options.Add("Ctrl (右)", (int)KeyCode.RightControl);
            options.Add("Alt (左)", (int)KeyCode.LeftAlt);
            options.Add("Alt (右)", (int)KeyCode.RightAlt);
            options.Add("Tab", (int)KeyCode.Tab);
            options.Add("Backspace", (int)KeyCode.Backspace);
            options.Add("Delete", (int)KeyCode.Delete);
            options.Add("Home", (int)KeyCode.Home);
            options.Add("End", (int)KeyCode.End);
            options.Add("PageUp", (int)KeyCode.PageUp);
            options.Add("PageDown", (int)KeyCode.PageDown);
            options.Add("插入", (int)KeyCode.Insert);
            // 方向键
            options.Add("向上", (int)KeyCode.UpArrow);
            options.Add("向下", (int)KeyCode.DownArrow);
            options.Add("向左", (int)KeyCode.LeftArrow);
            options.Add("向右", (int)KeyCode.RightArrow);
            // 鼠标按键
            options.Add("鼠标左键", (int)KeyCode.Mouse0);
            options.Add("鼠标右键", (int)KeyCode.Mouse1);
            options.Add("鼠标中键", (int)KeyCode.Mouse2);
            // 其他一些常用键
            options.Add("~", (int)KeyCode.BackQuote);
            options.Add("-", (int)KeyCode.Minus);
            options.Add("=", (int)KeyCode.Equals);
            options.Add("[", (int)KeyCode.LeftBracket);
            options.Add("]", (int)KeyCode.RightBracket);
            options.Add("\\", (int)KeyCode.Backslash);
            options.Add(";", (int)KeyCode.Semicolon);
            options.Add("'", (int)KeyCode.Quote);
            options.Add(",", (int)KeyCode.Comma);
            options.Add(".", (int)KeyCode.Period);
            options.Add("/", (int)KeyCode.Slash);
            return options;
        }
        private static string ToSingleFeatureChineseString(DamageFeature feature)
        {
            switch (feature)
            {
                case DamageFeature.Undefined:
                    return "未指定特性";
                case DamageFeature.NormalDamage:
                    return "普通伤害";
                case DamageFeature.RealDamage:
                    return "真实伤害";
                case DamageFeature.BuffOrEffectDamage:
                    return "增益/效果伤害";
                case DamageFeature.ArmorIgnoringDamage:
                    return "无视护甲伤害";
                case DamageFeature.CriticalDamage:
                    return "暴击伤害";
                case DamageFeature.ArmorPiercingDamage:
                    return "护甲穿透伤害";
                case DamageFeature.ExplosionDamage:
                    return "爆炸伤害";
                case DamageFeature.ArmorBreakingDamage:
                    return "护甲破坏伤害";
                case DamageFeature.ElementalDamage:
                    return "元素伤害";
                case DamageFeature.OnHitBuffApply:
                    return "命中附加增益";
                case DamageFeature.OnHitBleed:
                    return "命中附加流血";
                default:
                    return "未知特性"; // 处理未定义或将来添加的特性
            }
        }
    }
}