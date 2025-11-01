using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HideCharacter
{

    public class HideCharacterComponent : MonoBehaviour
    {
        public HideList? hideList = new HideList();
        private bool hide = false;
        private List<Renderer> rendererList = new  List<Renderer>();
        private bool needRefresh = true;
        private GameObject?
            bodyPartObject,
            tail,
            eye,
            eyebrow,
            mouth,
            hair,
            armLeft,
            armRight,
            thighLeft,
            thighRight,
            weapon,
            healthBar;

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            var dllDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
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
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            rendererList.Clear();
            var obj = GameObject.Find("ModelRoot");
            if (!obj) return;
            bodyPartObject = GameObject.Find("Pelvis");
            healthBar=GameObject.Find("HealthBars");
            needRefresh = true;
            //防止无法正常恢复身体
            foreach (var skinnedMeshRenderer in obj.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                rendererList.Add(skinnedMeshRenderer);
            }
        }

        void FindChildObjectsRecursively(Transform parentTransform)
        {
            foreach (Transform child in parentTransform)
            {
                switch (child.gameObject.name)
                {
                    case "TailSocket":
                        tail = child.gameObject;
                        break;
                    case "Thigh.L":
                        thighLeft = child.gameObject;
                        break;
                    case "Thigh.R":
                        thighRight = child.gameObject;
                        break;
                    case "HairSocket":
                        hair = child.gameObject;
                        break;
                    case "UpperArm.L":
                        armLeft = child.gameObject;
                        break;
                    case "UpperArm.R":
                        armRight = child.gameObject;
                        break;
                    case "MouthSocket":
                        mouth = child.gameObject;
                        break;
                    case "RightHandSocket":
                        weapon = child.gameObject;
                        break;
                    default:
                        if (child.gameObject.name.Contains("EyePart"))
                        {

                            eye = child.gameObject;

                        }
                        else if (child.gameObject.name.Contains("EyebrowPart"))
                        {

                            eyebrow = child.gameObject;

                        }

                        break;
                }

                FindChildObjectsRecursively(child);
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F5))
            {
                hide = !hide;
                SetCharacterHide(hide);
            }
        }

        private void SetCharacterHide(bool hide)
        {
            if (hideList != null)
            {
                if (needRefresh)
                {
                    if (bodyPartObject != null)
                        FindChildObjectsRecursively(bodyPartObject.transform);
					needRefresh=false;
                }
                tail?.SetActive(!(hide && hideList.hideTail));
                eye?.SetActive(!(hide && hideList.hideEyes));
                eyebrow?.SetActive(!(hide && hideList.hideEyebrow));
                mouth?.SetActive(!(hide && hideList.hideMouth));
                hair?.SetActive(!(hide && hideList.hideHair));
                armLeft?.SetActive(!(hide && hideList.hideArmLeft));
                armRight?.SetActive(!(hide && hideList.hideArmRight));
                thighLeft?.SetActive(!(hide && hideList.hideThighLeft));
                thighRight?.SetActive(!(hide && hideList.hideThighRight));
                weapon?.SetActive(!(hide && hideList.hideWeapon));
                healthBar?.SetActive(!(hide && hideList.hideHealthBar));

    
            }

            foreach (var o in rendererList)
            {
                o.enabled = !hide;
            }
        }
    }
}