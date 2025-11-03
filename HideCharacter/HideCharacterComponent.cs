using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Duckov.Utilities;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HideCharacter
{

    public class HideCharacterComponent : MonoBehaviour
    {

        public bool hide { get; private set; } = false;
        private List<Renderer> rendererList = new  List<Renderer>();
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
            healthBar,
            helmet,
            headTip,
            glasses,
            armor,
            backpack;

        private void OnEnable()
        {
            LevelManager.OnLevelInitialized+=OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
            
        }

        private void OnDisable()
        {
            LevelManager.OnLevelInitialized-=OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }

        private void OnSceneLoaded()
        {
            rendererList.Clear();
            hide = false;

            var obj = GameObject.Find("ModelRoot");
            if (!obj) return;
            //角色的模型是最先加载的，就直接先查找了，避免找到其他实体的身体
            bodyPartObject = GameObject.Find("Pelvis");
            healthBar = GameObject.Find("HealthBars");
            //身体的SkinnedMeshRenderer如果不隐藏会发现身体无法恢复
            foreach (var skinnedMeshRenderer in obj.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                rendererList.Add(skinnedMeshRenderer);
            }

            Refresh();
        }

        private void OnSceneUnloaded(Scene scene)
        {
            hide = false;
            SetCharacterHide(false);
        }

        public void Refresh()
        {
            tail = null;
            eye = null;
            eyebrow = null;
            mouth = null;
            hair = null;
            armLeft = null;
            armRight = null;
            thighLeft = null;
            thighRight = null;
            weapon = null;
            healthBar = null;
            helmet = null;
            glasses = null;
            headTip = null;
            armor = null;
            backpack = null;
            if (bodyPartObject != null)
                FindChildObjectsRecursively(bodyPartObject.transform);
        }
        /// <summary>
        /// 查找身体部件，不使用对Meshderer的隐藏是因为测试的时候发现没有正确隐藏，
        /// 可能是测试逻辑错了，就先这样写了
        /// </summary>
        /// <param name="parentTransform"></param>
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
                    case "HelmatSocket":
                        helmet= child.gameObject;
                        break;
                    case "FaceMaskSocket":
                        glasses = child.gameObject;
                        break;
                    case "HeadTip":
                        headTip= child.gameObject;
                        break;
                    case "ArmorSocket":
                        armor= child.gameObject;
                        break;
                    case "BackpackSocket":
                        backpack= child.gameObject;
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
            if (Input.GetKeyDown(ModBehaviour.hideList?.hotkey ?? KeyCode.F5))
            {
                hide = !hide;
                SetCharacterHide(hide);
            }
        }

        public void SetCharacterHide(bool hide)
        {
            var hideList = ModBehaviour.hideList;
            if (hideList != null)
            {
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
                helmet?.SetActive(!(hide && hideList.hideHelmet));
                glasses?.SetActive(!(hide && hideList.hideGlasses));
                headTip?.SetActive(!(hide && hideList.hideHeadTip));
                
                armor?.SetActive(!(hide && hideList.hideArmor));
                backpack?.SetActive(!(hide && hideList.hideBackpack));
            }

            foreach (var o in rendererList)
            {
                o.enabled = !hide;
            }
        }
    }
}