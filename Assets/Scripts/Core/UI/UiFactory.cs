using UnityEngine;
using UnityEngine.UI;

namespace SeaBattle.Core.UI
{
    public static class UiFactory
    {
        /// <summary>
        /// Creates a stretched rectangle under the parent.
        /// </summary>
        public static RectTransform CreateRect(Transform parent, string name)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return rect;
        }

        /// <summary>
        /// Creates a colored panel.
        /// </summary>
        public static RectTransform CreatePanel(Transform parent, string name, Color color)
        {
            var rect = CreateRect(parent, name);
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = White();
            image.color = color;
            return rect;
        }

        /// <summary>
        /// Creates a text label.
        /// </summary>
        public static Text CreateText(Transform parent, string content, int size, TextAnchor anchor)
        {
            var rect = CreateRect(parent, "Text");
            var text = rect.gameObject.AddComponent<Text>();
            text.font = Font();
            text.fontSize = size;
            text.alignment = anchor;
            text.color = Color.white;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.text = content;
            return text;
        }

        /// <summary>
        /// Creates a button with a centered label.
        /// </summary>
        public static Button CreateButton(Transform parent, string label)
        {
            var rect = CreatePanel(parent, label, new Color(0.2f, 0.32f, 0.48f));
            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = rect.GetComponent<Image>();
            var text = CreateText(rect, label, 16, TextAnchor.MiddleCenter);
            text.raycastTarget = false;
            return button;
        }

        /// <summary>
        /// Creates an integer field.
        /// </summary>
        public static InputField CreateIntegerField(Transform parent, string value)
        {
            var rect = CreatePanel(parent, "Delay", new Color(0.08f, 0.1f, 0.14f));
            var text = CreateText(rect, string.Empty, 16, TextAnchor.MiddleCenter);
            var input = rect.gameObject.AddComponent<InputField>();
            input.textComponent = text;
            input.contentType = InputField.ContentType.IntegerNumber;
            input.text = value;
            return input;
        }

        private static Font Font() => Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        private static Sprite White() => _sprite;

        private static readonly Sprite _sprite = CreateWhite();

        private static Sprite CreateWhite()
        {
            var texture = Texture2D.whiteTexture;
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }
    }
}
