using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace _TDS.UI
{
    public abstract class UiScreenRoot : MonoBehaviour
    {
        [SerializeField] private UiTheme theme;

        protected UiTheme Theme { get; private set; }
        protected RectTransform SafeArea { get; private set; }
        protected Transform ScreenContent => SafeArea;

        private bool ownsTheme;
        private Vector2Int lastScreenSize;

        protected virtual void Awake()
        {
            Theme = theme;
            if (Theme == null)
            {
                Theme = ScriptableObject.CreateInstance<UiTheme>();
                ownsTheme = true;
            }

            CreateCanvas();
            EnsureEventSystem();
            BuildScreen();
        }

        protected abstract void BuildScreen();

        protected void ClearScreen()
        {
            for (int i = SafeArea.childCount - 1; i >= 0; i--)
            {
                Destroy(SafeArea.GetChild(i).gameObject);
            }
        }

        protected GameObject CreateObject(string objectName, Transform parent)
        {
            GameObject gameObject = new GameObject(objectName, typeof(RectTransform));
            gameObject.transform.SetParent(parent, false);
            return gameObject;
        }

        protected UiPanel CreatePanel(string objectName, Transform parent, UiPanelTone tone)
        {
            GameObject gameObject = CreateObject(objectName, parent);
            gameObject.AddComponent<Image>();
            UiPanel panel = gameObject.AddComponent<UiPanel>();
            panel.Apply(Theme, tone);
            return panel;
        }

        protected UiText CreateText(
            string objectName,
            Transform parent,
            string value,
            UiTextRole role,
            TextAnchor alignment = TextAnchor.MiddleLeft)
        {
            GameObject gameObject = CreateObject(objectName, parent);
            Text text = gameObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = value ?? string.Empty;
            text.alignment = alignment;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            UiText styledText = gameObject.AddComponent<UiText>();
            styledText.Apply(Theme, role);
            return styledText;
        }

        protected UiButton CreateButton(
            string objectName,
            Transform parent,
            string label,
            UiButtonTone tone,
            UiTextRole textRole = UiTextRole.Label)
        {
            GameObject gameObject = CreateObject(objectName, parent);
            gameObject.AddComponent<Image>();
            UiButton button = gameObject.AddComponent<UiButton>();
            button.Apply(Theme, tone);

            UiText text = CreateText("Label", gameObject.transform, label, textRole, TextAnchor.MiddleCenter);
            text.GetComponent<Text>().raycastTarget = false;
            return button;
        }

        protected Image CreateDivider(string objectName, Transform parent, Color color)
        {
            GameObject gameObject = CreateObject(objectName, parent);
            Image image = gameObject.AddComponent<Image>();
            image.color = color;
            return image;
        }

        protected static void SetRect(
            RectTransform rect,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 offsetMin = default,
            Vector2 offsetMax = default)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            rect.localScale = Vector3.one;
        }

        protected static void SetSize(RectTransform rect, Vector2 size)
        {
            rect.sizeDelta = size;
        }

        private void CreateCanvas()
        {
            GameObject canvasObject = CreateObject("UiCanvas", transform);
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 200;

            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = Theme.ReferenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>();

            GameObject safeArea = CreateObject("SafeArea", canvasObject.transform);
            SafeArea = safeArea.GetComponent<RectTransform>();
            SetRect(SafeArea, Vector2.zero, Vector2.one);
            ApplySafeArea();
        }

        private void Update()
        {
            ApplySafeArea();
        }

        private void ApplySafeArea()
        {
            if (SafeArea == null || Screen.width <= 0 || Screen.height <= 0) return;

            Vector2Int screenSize = new Vector2Int(Screen.width, Screen.height);
            if (screenSize == lastScreenSize) return;
            lastScreenSize = screenSize;

            Rect area = Screen.safeArea;
            SafeArea.anchorMin = new Vector2(area.xMin / Screen.width, area.yMin / Screen.height);
            SafeArea.anchorMax = new Vector2(area.xMax / Screen.width, area.yMax / Screen.height);
            SafeArea.offsetMin = Vector2.zero;
            SafeArea.offsetMax = Vector2.zero;
        }

        private static void EnsureEventSystem()
        {
            if (EventSystem.current != null) return;
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        private void OnDestroy()
        {
            if (ownsTheme && Theme != null) Destroy(Theme);
        }
    }
}
