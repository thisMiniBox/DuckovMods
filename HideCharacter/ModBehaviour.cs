
using System;
using System.IO;
using System.Reflection;
using HarmonyLib;
using Newtonsoft.Json;
using UnityEngine;
using Object = UnityEngine.Object; // 确保引入 UnityEngine 命名空间

namespace HideCharacter
{
    public class ModBehaviour : Duckov.Modding.ModBehaviour 
    {
        public static HideCharacterComponent? hideHideCharacterManager=null;
        private const string CHILD_GAMEOBJECT_NAME = "HideCharacterManager";

        public const string MOD_ID = "HideCharacter";
        public const string MOD_NAME = "隐藏角色设置";
        private Harmony _harmony;

        public bool loadedConfigApi = false;
        public static HideList? hideList = new HideList();
        
        private void Awake()
        {
            loadedConfigApi = Api.ModConfigAPI.Initialize();
        }

        protected override void OnAfterSetup()
        {
            AddHideComponent();
            if (_harmony == null)
            {
                _harmony=new Harmony(MOD_ID);
                _harmony.PatchAll();
            }
            var dllDirectory = info.path;
            var configFilePath = Path.Combine(dllDirectory, "config.json");
            if (File.Exists(configFilePath))
            {
                try
                {
                    var jsonString = File.ReadAllText(configFilePath);
                    hideList = JsonConvert.DeserializeObject<HideList>(jsonString);
                }
                catch (JsonSerializationException ex) // 捕获 Newtonsoft.Json 特有的异常
                {
                    Debug.LogError($"JSON 反序列化错误 (Newtonsoft.Json): {ex.Message}");
                }
                catch (IOException ex)
                {
                    Debug.LogError($"文件读取错误: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"加载配置文件时发生未知错误: {ex.Message}");
                }
            }
            else
            {
                Debug.LogWarning($"配置文件 '{configFilePath}' 不存在。将使用默认设置。");
                try
                {
                    var jsonString = JsonConvert.SerializeObject(hideList, Formatting.Indented);
                    File.WriteAllText(configFilePath, jsonString);
                }
                catch (IOException ex)
                {
                    Debug.LogError($"创建配置文件时发生错误: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"创建配置文件时发生未知错误: {ex.Message}");
                }
            }
            if(Api.ModConfigAPI.IsAvailable())
                AddConfigSetting();
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

        private void AddConfigSetting()
        { 
            
            Api.ModConfigAPI.SafeAddBoolDropdownList(MOD_NAME,"hideArmor","隐藏装备",hideList.hideArmor);
            
        }

        private void OnConfigChange()
        {
            
        }
    }
}


