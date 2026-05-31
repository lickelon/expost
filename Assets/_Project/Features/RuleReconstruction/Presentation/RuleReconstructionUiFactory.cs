using UnityEngine;
using UnityEngine.UI;

namespace Expost.RuleReconstruction
{
    public sealed class RuleReconstructionUiFactory
    {
        private readonly Font font;
        private readonly Color buttonColor;

        public RuleReconstructionUiFactory(Font font, Color buttonColor)
        {
            this.font = font;
            this.buttonColor = buttonColor;
        }

        public RectTransform CreatePanel(string name, Transform parent, Color color)
        {
            var gameObject = new GameObject(name);
            gameObject.transform.SetParent(parent, false);
            var rectTransform = gameObject.AddComponent<RectTransform>();
            var image = gameObject.AddComponent<Image>();
            image.color = color;
            return rectTransform;
        }

        public Text CreateText(string name, Transform parent, string text, int fontSize, TextAnchor alignment)
        {
            var gameObject = new GameObject(name);
            gameObject.transform.SetParent(parent, false);
            var label = gameObject.AddComponent<Text>();
            label.font = font;
            label.text = text;
            label.fontSize = fontSize;
            label.alignment = alignment;
            label.color = Color.white;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Truncate;
            return label;
        }

        public Button CreateButton(string name, Transform parent, string label, int fontSize, UnityEngine.Events.UnityAction onClick)
        {
            var rectTransform = CreatePanel(name, parent, buttonColor);
            var button = rectTransform.gameObject.AddComponent<Button>();
            button.targetGraphic = rectTransform.GetComponent<Image>();
            button.onClick.AddListener(onClick);

            var text = CreateText("Text", rectTransform, label, fontSize, TextAnchor.MiddleCenter);
            Stretch(text.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            return button;
        }

        public Button CreateIconButton(string name, Transform parent, UnityEngine.Events.UnityAction onClick)
        {
            var rectTransform = CreatePanel(name, parent, buttonColor);
            var button = rectTransform.gameObject.AddComponent<Button>();
            button.targetGraphic = rectTransform.GetComponent<Image>();
            button.onClick.AddListener(onClick);
            return button;
        }

        public static void Stretch(RectTransform rectTransform, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.offsetMin = offsetMin;
            rectTransform.offsetMax = offsetMax;
        }

        public static void Anchor(RectTransform rectTransform, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            Stretch(rectTransform, anchorMin, anchorMax, offsetMin, offsetMax);
        }
    }
}
