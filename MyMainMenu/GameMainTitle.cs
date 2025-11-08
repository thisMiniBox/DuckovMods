using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MyMainMenu
{
    public class GameMainTitle
    {
        public string mainTitleObjName = "MainTitle";
        private GameObject? titleObject;
        private Image? logoImage;
        private Sprite? logoSprite;

        public void Initialize()
        {
            Debug.Log("Initialize method started.");
            // 查找所有 GameObject，并筛选出名称匹配的对象
            GameObject[] allObjects = Object.FindObjectsOfType<GameObject>();
            titleObject = Array.Find(allObjects, obj => obj.name == mainTitleObjName);
            if (titleObject == null)
            {
                Debug.LogWarning("titleObject not found with name: " + mainTitleObjName);
                return;
            }
            logoImage = titleObject.GetComponentInChildren<Image>(true);
            if (logoImage != null)
            {
                Debug.Log("Image component found on titleObject.");
                var texture = ImageLoader.LoadImageFromFile(@"C:\Users\Lenovo\Pictures\异噬.png");
                if (texture != null)
                {
                    Debug.Log("Texture loaded successfully.");
                    logoSprite=Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
                    logoImage.sprite = logoSprite;
                    Debug.Log("Sprite created and assigned to image component.");
                }
                else
                {
                    Debug.LogError("Failed to load texture from file: C:\\Users\\Lenovo\\Pictures\\异噬.png");
                }
            }
            else
            {
                Debug.LogWarning("Image component not found on titleObject.");
            }
            Debug.Log("Initialize method finished.");
        }

        public void SetTitle()
        {

        }

        public void Update()
        {
            if (logoImage!=null&&logoImage.sprite!=logoSprite)
            {
                Debug.Log("logoImage is not null and its sprite is different from logoSprite. Updating logoImage.sprite."); // Add Log inside the if condition
                logoImage.sprite = logoSprite;
            }
        }

    }
}