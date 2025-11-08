using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SceneView
{
    public class SceneViewPanel : MonoBehaviour, IDragHandler, IBeginDragHandler
    {
        public const float titleHeight = 50;

        private Vector2 shift;
        public ScrollRect? scrollRect;

        private TreeViewNode? treeViewNode;

        public string currentAimObj;

        private void Start()
        {
            currentAimObj = CanvasControl.ShowAim;
            CreateUI();
        }

        private void CreateUI()
        {
            CreateTitleBar();
            var scrollConfig = ScrollViewConfig.Default;
            scrollConfig.Horizontal = true;
            scrollConfig.SizeDelta = CanvasControl.panelSize - new Vector2(0, titleHeight);
            var scrollRectArr = ControlUtilities.CreateScrollView(transform, scrollConfig);
            scrollRect = scrollRectArr.scrollRect;
            if (scrollRect == null)
            {
                Debug.LogError("Failed to create ScrollView.");
                return;
            }

            var scrollRectRectTransform = scrollRect.GetComponent<RectTransform>();
            if (scrollRectRectTransform == null)
            {
                Debug.LogError("Failed to get RectTransform for ScrollView.");
                return;
            }

            scrollRectRectTransform.anchorMin = new Vector2(0, 0);
            scrollRectRectTransform.anchorMax = new Vector2(1, 1);
            scrollRectRectTransform.offsetMin = Vector2.zero;
            scrollRectRectTransform.offsetMax = new Vector2(0, -titleHeight);

            var content = scrollRect.content.gameObject;
            if (content == null)
            {
                Debug.LogError("Failed to get content for ScrollView.");
                return;
            }
            // var contentRectTransform = content.GetComponent<RectTransform>();
            // contentRectTransform.anchorMin = Vector2.up;
            // contentRectTransform.anchorMax = Vector2.one;
            // contentRectTransform.sizeDelta = Vector2.zero;
            // contentRectTransform.offsetMin = Vector2.zero;
            // contentRectTransform.offsetMax = new Vector2(0, 0);

            // var root = new GameObject("root");
            // root.AddComponent<RectTransform>();
            // root.transform.SetParent(content.transform, false);

            treeViewNode = content.AddComponent<TreeViewNode>();
            treeViewNode.UpdateHeight();

            // var vLayout = content.AddComponent<VerticalLayoutGroup>();
            // vLayout.padding = new RectOffset(2, 2, 2, 2);
            // vLayout.spacing = 5;
            // vLayout.childControlWidth = true;
            // vLayout.childControlHeight = false;
            // var size= content.AddComponent<ContentSizeFitter>();
            // size.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            // size.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            var inputConfig = InputFieldConfig.Default;
            inputConfig.RectConfig.AnchorMin = Vector2.zero;
            inputConfig.RectConfig.AnchorMax = Vector2.right;
            inputConfig.RectConfig.Pivot = new Vector2(0.5f, 1);
            inputConfig.RectConfig.SizeDelta = new Vector2(0, titleHeight);
            inputConfig.PlaceholderText = "输入搜索对象";
            var inputObj = ControlUtilities.CreateInputField(transform, inputConfig);
            inputObj.inputField.onEndEdit.AddListener(s =>
            {
                currentAimObj = s;
                Refresh();
            });


            Refresh();
        }

        private void CreateTitleBar()
        {
            var titleBar = new GameObject("TitleBar").AddComponent<RectTransform>();
            titleBar.transform.SetParent(transform, false);
            if (titleBar == null)
            {
                Debug.LogError("Failed to create TitleBar RectTransform.");
                return;
            }

            titleBar.anchorMin = new Vector2(0, 1);
            titleBar.anchorMax = new Vector2(1, 1);
            titleBar.pivot = new Vector2(0.5f, 1);
            titleBar.sizeDelta = new Vector2(0, titleHeight);
            titleBar.anchoredPosition = Vector2.zero;

            var titleTextConfig = TextConfig.Default;
            titleTextConfig.Text = "场景视图";
            titleTextConfig.RectConfig.AnchorMin = Vector2.zero;
            titleTextConfig.RectConfig.AnchorMax = Vector2.one;

            var titleText = ControlUtilities.CreateText(titleBar, titleTextConfig);
            if (titleText == null)
            {
                Debug.LogError("Failed to create TitleText.");
            }

            var refreshButtonConfig = ButtonConfig.Default;
            refreshButtonConfig.Text = "刷新";
            refreshButtonConfig.BackgroundColor = Color.yellow;
            refreshButtonConfig.RectConfig.SizeDelta = new Vector2(60, 40);
            refreshButtonConfig.RectConfig.AnchoredPosition = new Vector2(35, -25);
            var refreshButton = ControlUtilities.CreateButton(titleBar, refreshButtonConfig, Refresh);

            var closeButtonConfig = ButtonConfig.Default;
            closeButtonConfig.Text = "X";
            closeButtonConfig.BackgroundColor = Color.red;
            closeButtonConfig.RectConfig.AnchorMax = Vector2.one;
            closeButtonConfig.RectConfig.AnchorMin = Vector2.one;
            closeButtonConfig.RectConfig.SizeDelta = new Vector2(30, 30);
            closeButtonConfig.RectConfig.AnchoredPosition = new Vector2(-25, -25);
            ControlUtilities.CreateButton(titleBar, closeButtonConfig, () => gameObject.SetActive(false));
        }

        public void Refresh()
        {
            if (treeViewNode)
            {
                var canvas = FindIncludingHidden(currentAimObj);
                if (!canvas)
                {
                    Debug.LogError($"{currentAimObj} not found.");
                    return;
                }

                StartCoroutine(treeViewNode.DisplayGameObjectStructureCoroutine(canvas));
                // treeViewNode.DisplayGameObjectStructure(canvas);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            transform.position = eventData.position + shift;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            shift = transform.position - new Vector3(eventData.position.x, eventData.position.y);
        }

        /// <summary>
        /// 按名称查找场景中的GameObject，包括隐藏（非激活）对象和DontDestroyOnLoad对象。
        /// 此方法会遍历所有当前加载到内存中的GameObject实例。
        /// 
        /// **重要提示：**
        /// 1. **性能开销较大：** 该方法会遍历所有已加载的GameObject，性能开销相对较高。
        ///    因此，**不建议在Update、FixedUpdate等帧循环中频繁调用。**
        /// 2. **适用场景：** 更适合在初始化、加载场景、调试或不频繁的查找操作中使用。
        /// 3. **同名对象：** 如果场景中存在多个同名对象，此方法将返回它遇到的第一个匹配项。
        /// 4. **DontDestroyOnLoad：** 自动包含在DontDestroyOnLoad根下的对象。
        /// 5. **隐藏对象：** 自动包含Hierarchy中非激活（隐藏）的对象。
        /// </summary>
        /// <param name="name">要查找的GameObject的名称。</param>
        /// <returns>找到的第一个GameObject，如果未找到则返回null。</returns>
        public static GameObject FindIncludingHidden(string name)
        {
            // 1. 验证名称 - 处理null, empty, 或只包含空白字符的名称
            if (string.IsNullOrWhiteSpace(name))
            {
                Debug.LogWarning("FindIncludingHidden: Provided name is null, empty, or whitespace. Returning null.");
                return null;
            }

            // 2. 获取所有已加载的GameObject
            // Resources.FindObjectsOfTypeAll<GameObject>() 是实现需求的关键。
            // 它会找到所有当前加载到内存中的GameObject实例，不论它们是否激活，属于哪个场景（包括 DontDestroyOnLoad 场景），
            // 甚至可能包括一些编辑器内部使用的隐藏GameObject。
            GameObject[] allGameObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            // 3. 遍历并比较名称
            foreach (GameObject obj in allGameObjects)
            {
                // Unity中存在许多内部GameObject（例如：场景视图的相机、光照探头组等），
                // 它们的hideFlags属性可能被设置为HideInHierarchy、HideAndDontSave等。
                // 题目要求“包括隐藏”，并未明确排除这些内部或编辑器对象。
                // 因此，这里我们采取最宽松的策略：只要GameObject的name属性与传入的name匹配，就返回。
                // 如果未来需要排除特定类型的隐藏对象（例如只查找用户创建的游戏对象），
                // 可以根据 obj.scene.name 来过滤 DontDestroyOnLoad 场景中的对象，
                // 或者根据 obj.hideFlags 来排除编辑器内部对象。
                if (obj.name == name)
                {
                    return obj; // 4. 返回第一个匹配项
                }
            }

            // 5. 未找到则返回 null
            // 通常，查找函数在未找到结果时静默返回null是更常见的API行为。
            // 调用方可以根据返回值自行决定是否输出日志。
            // Debug.LogWarning($"FindIncludingHidden: GameObject with name '{name}' not found. Returning null.");
            return null;
        }

    }
}
