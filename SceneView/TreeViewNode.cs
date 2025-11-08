using System.Collections;
using SceneView;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SceneView
{
    public class TreeViewNode : MonoBehaviour
    {
        public TreeViewNode? parent = null;

        public RectTransform? rectTransform;
        public TextMeshProUGUI? text;
        public Button? label;
        public GameObject? child;
        public RectTransform? childRectTransform;
        public VerticalLayoutGroup? verticalLayout;
        public Button? objectEnable;
        public TextMeshProUGUI? objectText;

        public float buttonHeight = 50;

        private string originalText = "";



        private void Awake()
        {
            CreateUI();
        }

        [ContextMenu("CreateUI")]
        public void CreateUI()
        {
            // 获取 RectTransform 组件，如果不存在则添加
            if (rectTransform == null)
            {
                rectTransform = GetComponent<RectTransform>();
                if (rectTransform == null)
                {
                    rectTransform = gameObject.AddComponent<RectTransform>();
                }

                if (rectTransform == null)
                {
                    Debug.LogError("Failed to add RectTransform to the GameObject.");
                    return;
                }
            }

            rectTransform.sizeDelta = new Vector2(0, buttonHeight);
            rectTransform.anchorMin = Vector2.up;
            rectTransform.anchorMax = Vector2.one;

            var buttonConfig = ButtonConfig.Default;
            buttonConfig.RectConfig.Pivot = new Vector2(0.5f, 1);
            buttonConfig.RectConfig.AnchorMin = new Vector2(0, 1);
            buttonConfig.RectConfig.AnchorMax = new Vector2(1, 1);
            buttonConfig.RectConfig.SizeDelta = new Vector2(0, buttonHeight);

            // 创建按钮并检查是否已存在
            if (label == null)
            {
                var button = ControlUtilities.CreateButton(rectTransform, buttonConfig, Expand);
                label = button.button;
                text = button.text;
                if (text == null)
                {
                    Debug.LogError("Failed to get button text.");
                    return;
                }

                text.alignment = TextAlignmentOptions.MidlineLeft;
            }

            if (objectEnable == null)
            {
                var button = ControlUtilities.CreateButton(rectTransform, buttonConfig, null);
                objectEnable = button.button;
                objectText= button.text;
                button.text.text = "-";
                button.button.image.color = Color.yellow;
                var rect=objectEnable.GetComponent<RectTransform>();
                rect.anchorMin = Vector2.one;
                rect.anchorMax = Vector2.one;
                rect.pivot = Vector2.one / 2;
                rect.sizeDelta = new Vector2(30, 30);
                rect.anchoredPosition = new Vector2(-25, -25);
            }

            // 检查子对象是否存在
            child = transform.Find("content")?.gameObject;
            if (child == null)
            {
                child = new GameObject("content");
                child.transform.SetParent(rectTransform, false);
                if (child == null)
                {
                    Debug.LogError("Failed to create or set parent for child content.");
                    return;
                }

                childRectTransform = child.AddComponent<RectTransform>();
                if (childRectTransform == null)
                {
                    Debug.LogError("Failed to add RectTransform to the child content.");
                    return;
                }
            }
            else
            {
                childRectTransform = child.GetComponent<RectTransform>();
                if (childRectTransform == null)
                {
                    Debug.LogError("Failed to get RectTransform for the child content.");
                    return;
                }
            }

            childRectTransform.anchorMin = new Vector2(0, 1);
            childRectTransform.anchorMax = new Vector2(1, 1);
            childRectTransform.pivot = new Vector2(0.5f, 1);
            childRectTransform.offsetMax = new Vector2(0, -buttonHeight);
            childRectTransform.offsetMin = new Vector2(0, 0);

            verticalLayout = child.GetComponent<VerticalLayoutGroup>();
            var sizeFitter = child.GetComponent<ContentSizeFitter>();

            if (verticalLayout == null)
            {
                verticalLayout = child.AddComponent<VerticalLayoutGroup>();
                if (verticalLayout == null)
                {
                    Debug.LogError("Failed to add VerticalLayoutGroup to the child content.");
                    return;
                }
            }

            if (sizeFitter == null)
            {
                sizeFitter = child.AddComponent<ContentSizeFitter>();
                if (sizeFitter == null)
                {
                    Debug.LogError("Failed to add ContentSizeFitter to the child content.");
                    return;
                }
            }

            verticalLayout.padding.top = 2;
            verticalLayout.padding.bottom = 4;
            verticalLayout.spacing = 2;
            verticalLayout.childControlHeight = false;
            verticalLayout.childControlWidth = true;

            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            
        }

        public void InsertChild(RectTransform childRectTransform)
        {
            if (child != null && childRectTransform != null)
            {
                // 将子节点添加到 content 容器中
                childRectTransform.SetParent(child.transform, false);
            }
        }

        private void Expand()
        {
            if (child == null)
            {
                Debug.LogError("Child content not found.");
                return;
            }

            child.SetActive(!child.activeSelf);

            UpdateHeight();

            // 更新名称以模拟动画效果
            if (child.activeSelf)
            {
                if (text != null) text.text = originalText + " ↓"; // 下三角符号
            }
            else
            {
                if (text != null) text.text = originalText + " →"; // 右三角符号
            }
        }

        public void UpdateHeight()
        {
            if (child == null || childRectTransform == null || rectTransform == null)
            {
                Debug.LogError("Failed to update height. Child, ChildRectTransform, or RectTransform is null.");
                return;
            }

            if (child.activeSelf)
            {
                rectTransform.sizeDelta =
                    new Vector2(rectTransform.sizeDelta.x, buttonHeight + childRectTransform.sizeDelta.y);
            }
            else
            {
                rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, buttonHeight);
            }

            if (parent != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(parent.childRectTransform);
                parent.UpdateHeight();
            }
        }

        public IEnumerator DisplayGameObjectStructureCoroutine(GameObject targetObject, int depth = 0)
        {
            if (targetObject == null)
            {
                Debug.LogError("Target object is null.");
                yield break;
            }

            if (targetObject.name == CanvasControl.ViewCanvasName)
                yield break;
            
            
            ClearChildNodes();
            originalText = GetIndentedName(targetObject.name, depth);
            if (text != null) text.text = originalText;
            objectEnable.onClick.AddListener(() => OnObjectEnable(targetObject, objectEnable, objectText));
            UpdateObjButton(targetObject,objectEnable,objectText);


            var components = targetObject.GetComponents<Behaviour>();
            foreach (var component in components)
            {
                if (childRectTransform == null)
                {
                    Debug.LogError("childRectTransform is null");
                    continue;
                }

                var config = ButtonConfig.Default;
                config.RectConfig.SizeDelta = new Vector2(0, 40);
                config.BackgroundColor = Color.green;
                config.Text = new string(' ', (depth + 1) * 2) + component.GetType().Name;
                var buttonBack = ControlUtilities.CreateButton(childRectTransform, config, null);
                if (buttonBack.button == null)
                {
                    Debug.LogError("buttonBack.button is null");
                    continue;
                }

                var button = buttonBack.button;
                var comp = component;
                buttonBack.button.onClick.AddListener(() => OnButtonClick(button, comp));
                yield return null;
            }

            // 计数器，用于跟踪子协程的数量
            var childCount = targetObject.transform.childCount;
            var completedChildren = 0;

            var allChildrenExpanded = false;

            // 遍历目标对象的所有子对象
            foreach (Transform childTransform in targetObject.transform)
            {
                if (childTransform == null)
                {
                    Debug.LogError("Child transform is null.");
                    continue;
                }

                var childNode = new GameObject($"{childTransform.gameObject.name}").AddComponent<TreeViewNode>();
                childNode.transform.SetParent(childRectTransform, false);
                childNode.parent = this;
                childNode.CreateUI();

                StartCoroutine(childNode.DisplayGameObjectStructureCoroutine(childTransform.gameObject, depth + 1, () =>
                {
                    completedChildren++;
                    if (completedChildren == childCount)
                    {
                        allChildrenExpanded = true;
                    }
                }));

                yield return null;
            }

            // 等待所有子协程完成
            while (!allChildrenExpanded && childCount > 0)
            {
                yield return null;
            }

            // 在所有子节点初始化完毕后调用Expand
            Expand();
        }

        public IEnumerator DisplayGameObjectStructureCoroutine(GameObject targetObject, int depth,
            System.Action onComplete)
        {
            if (targetObject == null)
            {
                Debug.LogError("Target object is null.");
                yield break;
            }

            ClearChildNodes();
            originalText = GetIndentedName(targetObject.name, depth);
            if (text != null) text.text = originalText;
            objectEnable.onClick.AddListener(() => OnObjectEnable(targetObject, objectEnable, objectText));
            UpdateObjButton(targetObject,objectEnable,objectText);
            var components = targetObject.GetComponents<Behaviour>();
            foreach (var component in components)
            {
                if (childRectTransform == null)
                {
                    Debug.LogError("childRectTransform is null");
                    continue;
                }

                var config = ButtonConfig.Default;
                config.RectConfig.SizeDelta = new Vector2(0, 40);
                config.BackgroundColor = Color.green;
                config.Text = new string(' ', (depth + 1) * 2) + component.GetType().Name;
                var buttonBack = ControlUtilities.CreateButton(childRectTransform, config, null);
                if (buttonBack.button == null)
                {
                    Debug.LogError("buttonBack.button is null");
                    continue;
                }

                var button = buttonBack.button;
                var comp = component;
                buttonBack.button.onClick.AddListener(() => OnButtonClick(button, comp));
                yield return null;
            }

            // 计数器，用于跟踪子协程的数量
            var childCount = targetObject.transform.childCount;
            var completedChildren = 0;

            var allChildrenExpanded = false;

            // 遍历目标对象的所有子对象
            foreach (Transform childTransform in targetObject.transform)
            {
                if (childTransform == null)
                {
                    Debug.LogError("Child transform is null.");
                    continue;
                }

                var childNode = new GameObject($"{childTransform.gameObject.name}").AddComponent<TreeViewNode>();
                childNode.transform.SetParent(childRectTransform, false);
                childNode.parent = this;
                childNode.CreateUI();

                StartCoroutine(childNode.DisplayGameObjectStructureCoroutine(childTransform.gameObject, depth + 1, () =>
                {
                    completedChildren++;
                    if (completedChildren == childCount)
                    {
                        allChildrenExpanded = true;
                    }
                }));

                yield return null;
            }

            // 等待所有子协程完成
            while (!allChildrenExpanded && childCount > 0)
            {
                yield return null;
            }

            // 在所有子节点初始化完毕后调用Expand
            Expand();

            // 调用完成回调
            onComplete?.Invoke();
        }

        private string GetIndentedName(string name, int depth)
        {
            var indent = "";
            for (int i = 0; i < depth; i++)
            {
                indent += $"{i}_";
            }
            return $"{indent}_{name}";
        }

        public void ClearChildNodes()
        {
            if (childRectTransform != null)
                foreach (Transform childRectTransform in this.childRectTransform)
                {
                    Destroy(childRectTransform.gameObject);
                }
        }

        private void OnButtonClick(Button button, Behaviour component)
        {
            if (button == null)
            {
                Debug.LogError("button is null");
                return;
            }

            if (component == null)
            {
                Debug.LogError("component is null");
                return;
            }

            // 切换组件的启用状态
            var isEnabled = !component.enabled;
            component.enabled = isEnabled;
            // 根据组件的启用状态修改按钮背景颜色
            button.image.color = isEnabled ? Color.green : Color.red;
        }

        private void OnObjectEnable(GameObject gameObject, Button button, TMP_Text text)
        {
            Debug.Log($"设置{gameObject.name}={!gameObject.activeSelf}");
            gameObject.SetActive(!gameObject.activeSelf);
            UpdateObjButton(gameObject, button, text);
        }

        private void UpdateObjButton(GameObject gameObject, Button button, TMP_Text text)
        {
            if (gameObject.activeSelf)
            {
                button.image.color = Color.green;
                text.text = "√";
            }
            else
            {
                button.image.color = Color.red;
                text.text = "×";
            }
        }
    }
}
