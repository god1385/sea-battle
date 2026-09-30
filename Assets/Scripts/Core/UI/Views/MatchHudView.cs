using System;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace SeaBattle.Core.UI.Views
{
    public class MatchHudView : IMatchHudView
    {
        private readonly Button _restart;
        private readonly Toggle _logToggle;
        private readonly Text _log;

        /// <summary>
        /// Builds the canvas, the shared debug bar, and the two column hosts.
        /// </summary>
        public MatchHudView(Transform parent, bool logEnabled)
        {
            CreateEventSystem(parent);
            var canvas = CreateCanvas(parent);
            var background = UiFactory.CreatePanel(canvas, "Background", new Color(0.04f, 0.06f, 0.1f));
            var layout = background.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(16, 16, 16, 16);
            layout.spacing = 12;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var top = CreateRow(background, 40f);
            var topRow = top.gameObject.AddComponent<HorizontalLayoutGroup>();
            topRow.spacing = 12f;
            topRow.childControlWidth = true;
            topRow.childControlHeight = true;
            topRow.childForceExpandWidth = true;
            topRow.childForceExpandHeight = true;
            _restart = UiFactory.CreateButton(top, "Перезапустить сцену");
            _logToggle = CreateLogToggle(top, logEnabled);

            var columns = CreateRow(background, 760f);
            var row = columns.gameObject.AddComponent<HorizontalLayoutGroup>();
            row.spacing = 16;
            row.childControlWidth = true;
            row.childControlHeight = true;
            row.childForceExpandWidth = true;
            row.childForceExpandHeight = true;
            FirstColumn = columns;
            SecondColumn = columns;

            var logHost = CreateRow(background, 80f);
            _log = UiFactory.CreateText(logHost, string.Empty, 14, TextAnchor.UpperLeft);
        }

        public RectTransform FirstColumn { get; }

        public RectTransform SecondColumn { get; }

        public IObservable<Unit> Restart => _restart.OnClickAsObservable();

        public IObservable<bool> LogToggle => _logToggle.OnValueChangedAsObservable();

        /// <summary>
        /// Replaces the traffic log text.
        /// </summary>
        public void ShowLog(string text) => _log.text = text;

        private static void CreateEventSystem(Transform parent)
        {
            var system = new GameObject("EventSystem");
            system.transform.SetParent(parent, false);
            system.AddComponent<EventSystem>();
            system.AddComponent<InputSystemUIInputModule>();
        }

        private static RectTransform CreateCanvas(Transform parent)
        {
            var rect = new GameObject("Canvas", typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            var canvas = rect.gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = rect.gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1600f, 900f);
            rect.gameObject.AddComponent<GraphicRaycaster>();
            return rect;
        }

        private static RectTransform CreateRow(Transform parent, float height)
        {
            var row = UiFactory.CreateRect(parent, "Row");
            var element = row.gameObject.AddComponent<LayoutElement>();
            element.minHeight = height;
            element.preferredHeight = height;
            element.flexibleWidth = 1f;
            return row;
        }

        private static Toggle CreateLogToggle(Transform parent, bool enabled)
        {
            var panel = UiFactory.CreatePanel(parent, "LogToggle", new Color(0.2f, 0.32f, 0.48f));
            var toggle = panel.gameObject.AddComponent<Toggle>();
            var mark = UiFactory.CreatePanel(panel, "Mark", new Color(0.95f, 0.78f, 0.2f));
            mark.anchorMin = new Vector2(0f, 0.2f);
            mark.anchorMax = new Vector2(0f, 0.8f);
            mark.pivot = new Vector2(0f, 0.5f);
            mark.sizeDelta = new Vector2(18f, 0f);
            mark.anchoredPosition = new Vector2(10f, 0f);
            toggle.targetGraphic = panel.GetComponent<Image>();
            toggle.graphic = mark.GetComponent<Image>();
            toggle.isOn = enabled;
            var label = UiFactory.CreateText(panel, "Лог сообщений", 16, TextAnchor.MiddleCenter);
            label.rectTransform.offsetMin = new Vector2(36f, 0f);
            label.raycastTarget = false;
            return toggle;
        }
    }
}
