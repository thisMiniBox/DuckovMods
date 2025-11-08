using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SceneView
{
    [Serializable]
    public struct RectTransformConfig
    {
        public Vector2 AnchorMin;
        public Vector2 AnchorMax;
        public Vector2 AnchoredPosition;
        public Vector2 SizeDelta;
        public Vector2 OffsetMin;
        public Vector2 OffsetMax;
        public Vector2 Pivot;

        // 默认配置
        public static readonly RectTransformConfig Default = new RectTransformConfig(
            anchorMin: new Vector2(0, 1),
            anchorMax: new Vector2(0, 1),
            anchoredPosition: Vector2.zero,
            sizeDelta: Vector2.zero,
            offsetMin: Vector2.zero,
            offsetMax: Vector2.zero,
            pivot: new Vector2(0.5f, 0.5f) // 默认居中轴心点
        );

        public RectTransformConfig(
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 anchoredPosition,
            Vector2 sizeDelta,
            Vector2 offsetMin,
            Vector2 offsetMax,
            Vector2 pivot)
        {
            AnchorMin = anchorMin;
            AnchorMax = anchorMax;
            AnchoredPosition = anchoredPosition;
            SizeDelta = sizeDelta;
            OffsetMin = offsetMin;
            OffsetMax = offsetMax;
            Pivot = pivot;
        }
    }

    [Serializable]
    public struct ButtonConfig
    {
        public RectTransformConfig RectConfig;
        public Color BackgroundColor;
        public string Text;
        public int FontSize;
        public Color TextColor;
        public bool RaycastTarget;

        public static readonly ButtonConfig Default = new ButtonConfig(
            rectConfig: RectTransformConfig.Default,
            backgroundColor: new Color(0.2f, 0.2f, 0.2f, 1f),
            text: "Button",
            fontSize: 18,
            textColor: Color.white,
            raycastTarget: true
        );

        public ButtonConfig(
            RectTransformConfig rectConfig,
            Color backgroundColor,
            string text,
            int fontSize,
            Color textColor,
            bool raycastTarget = true)
        {
            RectConfig = rectConfig;
            BackgroundColor = backgroundColor;
            Text = text;
            FontSize = fontSize;
            TextColor = textColor;
            RaycastTarget = raycastTarget;
        }
    }

    [Serializable]
    public struct TextConfig
    {
        public RectTransformConfig RectConfig;
        public string Text;
        public int FontSize;
        public Color TextColor;
        public TextAlignmentOptions Alignment;
        public bool RaycastTarget;

        public static readonly TextConfig Default = new TextConfig(
            rectConfig: RectTransformConfig.Default,
            text: "New Text",
            fontSize: 18,
            textColor: Color.white,
            alignment: TextAlignmentOptions.Center,
            raycastTarget: false
        );

        public TextConfig(
            string text,
            int fontSize,
            Color textColor,
            TextAlignmentOptions alignment,
            bool raycastTarget,
            RectTransformConfig rectConfig)
        {
            RectConfig = rectConfig;
            Text = text;
            FontSize = fontSize;
            TextColor = textColor;
            Alignment = alignment;
            RaycastTarget = raycastTarget;
        }
    }

    [Serializable]
    public struct ScrollViewConfig
    {
        public Vector2 SizeDelta; // ScrollView 的尺寸
        public bool Vertical;
        public bool Horizontal;
        public Color BackgroundColor;
        public Vector2 ContentPadding; // 内容区域的内边距（上下左右）

        public static readonly ScrollViewConfig Default = new ScrollViewConfig
        {
            SizeDelta = new Vector2(400, 300),
            Vertical = true,
            Horizontal = false,
            BackgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.8f),
            ContentPadding = new Vector2(10, 10) // (horizontal, vertical)
        };
    }

    [Serializable]
    public struct InputFieldConfig
    {
        public RectTransformConfig RectConfig;
        public Color BackgroundColor;
        public string PlaceholderText;
        public int PlaceholderFontSize;
        public Color PlaceholderTextColor;
        public Color TextColor;
        public int FontSize;
        public TextAlignmentOptions TextAlignment;
        public TMP_InputField.CharacterValidation CharacterValidation;
        public int CharacterLimit;
        public static readonly InputFieldConfig Default = new InputFieldConfig(
            rectConfig: RectTransformConfig.Default,
            backgroundColor: new Color(0.2f, 0.2f, 0.2f, 1f),
            placeholderText: "Enter text here",
            placeholderFontSize: 14,
            placeholderTextColor: new Color(0.7f, 0.7f, 0.7f, 1f),
            textColor: Color.white,
            fontSize: 18,
            textAlignment: TextAlignmentOptions.Left,
            characterValidation: TMP_InputField.CharacterValidation.None,
            characterLimit: 0
        );
        public InputFieldConfig(
            RectTransformConfig rectConfig,
            Color backgroundColor,
            string placeholderText,
            int placeholderFontSize,
            Color placeholderTextColor,
            Color textColor,
            int fontSize,
            TextAlignmentOptions textAlignment,
            TMP_InputField.CharacterValidation characterValidation = TMP_InputField.CharacterValidation.None,
            int characterLimit = 0)
        {
            RectConfig = rectConfig;
            BackgroundColor = backgroundColor;
            PlaceholderText = placeholderText;
            PlaceholderFontSize = placeholderFontSize;
            PlaceholderTextColor = placeholderTextColor;
            TextColor = textColor;
            FontSize = fontSize;
            TextAlignment = textAlignment;
            CharacterValidation = characterValidation;
            CharacterLimit = characterLimit;
        }
    }

    public static class ControlUtilities
    {
        // 通用方法：将 RectTransformConfig 应用于 RectTransform
        private static void ApplyRectTransformConfig(RectTransform rectTransform, RectTransformConfig config)
        {
            rectTransform.anchorMin = config.AnchorMin;
            rectTransform.anchorMax = config.AnchorMax;
            rectTransform.pivot = config.Pivot;

            var isStretched = config.SizeDelta == Vector2.zero;

            if (isStretched)
            {
                rectTransform.offsetMin = config.OffsetMin;
                rectTransform.offsetMax = config.OffsetMax;
            }
            else
            {
                rectTransform.anchoredPosition = config.AnchoredPosition;
                rectTransform.sizeDelta = config.SizeDelta;
            }
        }

        // ========================
        // 按钮创建
        // ========================
        public static (Button? button, TextMeshProUGUI? text) CreateButton(RectTransform? parent, ButtonConfig config,
            UnityAction? onClick)
        {
            var btnObj = new GameObject(config.Text + "Button");
            var btnRect = btnObj.AddComponent<RectTransform>();
            btnRect.SetParent(parent, false);

            ApplyRectTransformConfig(btnRect, config.RectConfig);

            var button = btnObj.AddComponent<Button>();
            var image = btnObj.AddComponent<Image>();
            image.color = config.BackgroundColor;
            button.image = image;

            // 创建文本子对象
            var txtObj = new GameObject("Text (TMP)");
            var txtRect = txtObj.AddComponent<RectTransform>();
            txtRect.SetParent(btnRect, false);

            // 文本始终填满按钮（常见做法）
            ApplyRectTransformConfig(txtRect, new RectTransformConfig(
                anchorMin: Vector2.zero,
                anchorMax: Vector2.one,
                anchoredPosition: Vector2.zero,
                sizeDelta: Vector2.zero,
                offsetMin: Vector2.zero,
                offsetMax: Vector2.zero,
                pivot: new Vector2(0.5f, 0.5f)
            ));

            var tmpText = txtObj.AddComponent<TextMeshProUGUI>();
            tmpText.text = config.Text;
            tmpText.color = config.TextColor;
            tmpText.alignment = TextAlignmentOptions.Center;
            tmpText.fontSize = config.FontSize;
            tmpText.raycastTarget = config.RaycastTarget;
            if (onClick != null)
                button.onClick.AddListener(onClick);
            return (button, tmpText);
        }

        // ========================
        // 文本创建（重载）
        // ========================
        public static TextMeshProUGUI CreateText(Transform parent, string text)
        {
            var config = TextConfig.Default;
            config.Text = text;
            return CreateText(parent, config);
        }

        public static TextMeshProUGUI CreateText(Transform parent, TextConfig config)
        {
            var textObj = new GameObject(config.Text + "Text");
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.SetParent(parent, false);

            ApplyRectTransformConfig(textRect, config.RectConfig);

            var tmpText = textObj.AddComponent<TextMeshProUGUI>();
            tmpText.text = config.Text;
            tmpText.color = config.TextColor;
            tmpText.alignment = config.Alignment;
            tmpText.fontSize = config.FontSize;
            tmpText.raycastTarget = config.RaycastTarget;

            return tmpText;
        }



        // ========================
        // 滚动视图创建
        // ========================
        public static (ScrollRect scrollRect, RectTransform content) CreateScrollView(
            Transform parent,
            ScrollViewConfig config)
        {
            // 1. ScrollView 根对象
            var scrollViewObj = new GameObject("ScrollView");
            var scrollViewRect = scrollViewObj.AddComponent<RectTransform>();
            scrollViewRect.SetParent(parent, false);
            scrollViewRect.sizeDelta = config.SizeDelta;

            var scrollView = scrollViewObj.AddComponent<ScrollRect>();
            var scrollViewImage = scrollViewObj.AddComponent<Image>();
            scrollViewImage.color = config.BackgroundColor;
            scrollViewImage.raycastTarget = true;

            // 2. Viewport 子对象
            var viewportObj = new GameObject("Viewport");
            var viewportRect = viewportObj.AddComponent<RectTransform>();
            viewportRect.SetParent(scrollViewRect, false);
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = Vector2.zero;
            viewportRect.offsetMax = Vector2.zero;
            viewportRect.pivot = new Vector2(0.5f, 0.5f);

            var viewportMask = viewportObj.AddComponent<RectMask2D>(); // 或 Mask，但 RectMask2D 性能更好
            var viewportImage = viewportObj.AddComponent<Image>();
            viewportImage.color = Color.clear; // 透明背景

            // 3. Content 子对象
            var contentObj = new GameObject("Content");
            var contentRect = contentObj.AddComponent<RectTransform>();
            contentRect.SetParent(viewportRect, false);

            // 设置 Content 的锚点：根据滚动方向决定
            if (config.Vertical && !config.Horizontal)
            {
                // 垂直滚动：宽度拉满，高度自适应
                contentRect.anchorMin = new Vector2(0, 1);
                contentRect.anchorMax = new Vector2(1, 1);
                contentRect.pivot = new Vector2(0.5f, 1);
                contentRect.anchoredPosition = new Vector2(0, -config.ContentPadding.y);
                contentRect.sizeDelta = new Vector2(-2 * config.ContentPadding.x, 0); // 左右留边距
            }
            else if (config.Horizontal && !config.Vertical)
            {
                // 水平滚动：高度拉满，宽度自适应
                contentRect.anchorMin = new Vector2(0, 0);
                contentRect.anchorMax = new Vector2(0, 1);
                contentRect.pivot = new Vector2(0, 0.5f);
                contentRect.anchoredPosition = new Vector2(config.ContentPadding.x, 0);
                contentRect.sizeDelta = new Vector2(0, -2 * config.ContentPadding.y);
            }
            else
            {
                // 双向滚动：自由布局
                contentRect.anchorMin = Vector2.zero;
                contentRect.anchorMax = Vector2.zero;
                contentRect.pivot = new Vector2(0, 1);
                contentRect.anchoredPosition = Vector2.zero;
                contentRect.sizeDelta = Vector2.zero;
            }

            // 4. 关联 ScrollRect
            scrollView.viewport = viewportRect;
            scrollView.content = contentRect;
            scrollView.vertical = config.Vertical;
            scrollView.horizontal = config.Horizontal;
            scrollView.movementType = ScrollRect.MovementType.Elastic;
            scrollView.inertia = true;
            scrollView.decelerationRate = 0.135f; // 默认值

            return (scrollView, contentRect);
        }

        // ========================
        // 文本编辑器创建
        // ========================
        public static (TMP_InputField inputField, TextMeshProUGUI text) CreateInputField(Transform parent,
            InputFieldConfig config)
        {
            var inputFieldObj = new GameObject("InputField");
            var inputFieldRect = inputFieldObj.AddComponent<RectTransform>();
            inputFieldRect.SetParent(parent, false);
            ApplyRectTransformConfig(inputFieldRect, config.RectConfig);
            var inputField = inputFieldObj.AddComponent<TMP_InputField>();
            // 背景图像
            var backgroundImage = inputFieldObj.AddComponent<Image>();
            backgroundImage.sprite = Resources.Load<Sprite>("UI/Skins/Background");
            backgroundImage.type = Image.Type.Sliced;
            backgroundImage.color = config.BackgroundColor;
            // 输入框文本组件
            var textObj = new GameObject("Text (TMP)");
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.SetParent(inputFieldRect, false);
            // 文本始终填满输入框
            ApplyRectTransformConfig(textRect, new RectTransformConfig(
                anchorMin: Vector2.zero,
                anchorMax: Vector2.one,
                anchoredPosition: Vector2.zero,
                sizeDelta: Vector2.zero,
                offsetMin: Vector2.zero,
                offsetMax: Vector2.zero,
                pivot: new Vector2(0.5f, 0.5f)
            ));
            var tmpText = textObj.AddComponent<TextMeshProUGUI>();
            tmpText.text = "";
            tmpText.color = config.TextColor;
            tmpText.alignment = config.TextAlignment;
            tmpText.fontSize = config.FontSize;
            tmpText.raycastTarget = false;
            inputField.textComponent = tmpText;
            // 占位符文本组件
            var placeholderObj = new GameObject("Placeholder");
            var placeholderRect = placeholderObj.AddComponent<RectTransform>();
            placeholderRect.SetParent(inputFieldRect, false);
            // 占位符文本始终填满输入框
            ApplyRectTransformConfig(placeholderRect, new RectTransformConfig(
                anchorMin: Vector2.zero,
                anchorMax: Vector2.one,
                anchoredPosition: Vector2.zero,
                sizeDelta: Vector2.zero,
                offsetMin: Vector2.zero,
                offsetMax: Vector2.zero,
                pivot: new Vector2(0.5f, 0.5f)
            ));
            var placeholderText = placeholderObj.AddComponent<TextMeshProUGUI>();
            placeholderText.text = config.PlaceholderText;
            placeholderText.color = config.PlaceholderTextColor;
            placeholderText.alignment = config.TextAlignment;
            placeholderText.fontSize = config.PlaceholderFontSize;
            placeholderText.raycastTarget = false;
            inputField.placeholder = placeholderText;
            // 配置输入字段属性
            inputField.characterValidation = config.CharacterValidation;
            inputField.characterLimit = config.CharacterLimit;
            return (inputField, tmpText);
        }
    }
}

        