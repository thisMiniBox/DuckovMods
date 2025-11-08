using System;
using UnityEngine;
using UnityEngine.UI;

namespace SceneView
{
    public class CanvasControl : MonoBehaviour
    {
        public SceneViewPanel? sceneViewPanel;
        
        public static Vector2 panelSize = new Vector2(500, 800);

        public const string ViewCanvasName = "_SceneViewCanvas";
        public const string ShowAim = "LOGO";
            
        private void Start()
        {
            InitCanvas();
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.F2))
            {
                Debug.Log("切换");
                sceneViewPanel.gameObject.SetActive(!sceneViewPanel.gameObject.activeSelf);
                if (sceneViewPanel.gameObject.activeSelf)
                {
                    sceneViewPanel.Refresh();
                }
            }
        }

        private void InitCanvas()
        {
            var canvasObj = new GameObject(ViewCanvasName);
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
            DontDestroyOnLoad(canvasObj);

            sceneViewPanel=CreateSceneViewPanel(canvasObj.transform);
        }
        public static SceneViewPanel CreateSceneViewPanel(Transform parent)
        {
            var panelObj = new GameObject("SceneViewPanel");
            panelObj.transform.SetParent(parent, false);
            var sceneViewPanel = panelObj.AddComponent<SceneViewPanel>();
            var panelImage = panelObj.AddComponent<Image>();
            panelImage.color = new Color(0.5f, 0.5f, 0.5f, 0.3f);

            var rectTransform = panelObj.GetComponent<RectTransform>();
            rectTransform.sizeDelta = panelSize;

            return sceneViewPanel;
        }
    }
}