using _GameToolkit.Startup;
using _TDS.Gameplay;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace _TDS.Home
{
    [ExecuteAlways]
    public class HomeScene : MonoBehaviour
    {
        private static readonly Color BackgroundColor = ParseColor("0B1220");
        private static readonly Color PanelColor = ParseColor("172238");
        private static readonly Color AccentColor = ParseColor("5EEAD4");
        private static readonly Color TextColor = ParseColor("F8FAFC");
        private static readonly Color MutedColor = ParseColor("A8B5C7");

        private Font font;

        private void Awake()
        {
            BuildUi();
        }

        private void OnEnable()
        {
            if (!Application.isPlaying)
            {
                BuildUi();
            }
        }

        private void BuildUi()
        {
            if (transform.Find("HomeCanvas") != null)
            {
                BindButtons();
                return;
            }

            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            EnsureEventSystem();

            Canvas canvas = CreateCanvas();
            RectTransform root = canvas.GetComponent<RectTransform>();

            Image background = CreateImage("Background", root, BackgroundColor);
            Stretch(background.rectTransform);

            Image panel = CreateImage("Panel", root, PanelColor);
            SetRect(panel.rectTransform, new Vector2(0.08f, 0.1f), new Vector2(0.92f, 0.9f), Vector2.zero, Vector2.zero);

            CreateText("Title", panel.rectTransform, "IDLE // CIRCUIT", 34, TextColor,
                new Vector2(0.08f, 0.78f), new Vector2(0.92f, 0.94f), TextAnchor.MiddleCenter);
            CreateText("Subtitle", panel.rectTransform, "SELECT A LEVEL", 16, MutedColor,
                new Vector2(0.08f, 0.68f), new Vector2(0.92f, 0.78f), TextAnchor.MiddleCenter);

            GameObject levelRow = CreateObject("LevelRow", panel.rectTransform);
            RectTransform rowRect = levelRow.GetComponent<RectTransform>();
            SetRect(rowRect, new Vector2(0.06f, 0.26f), new Vector2(0.94f, 0.66f), Vector2.zero, Vector2.zero);
            HorizontalLayoutGroup row = levelRow.AddComponent<HorizontalLayoutGroup>();
            row.spacing = 12f;
            row.padding = new RectOffset(4, 4, 4, 4);
            row.childForceExpandWidth = true;
            row.childForceExpandHeight = true;
            row.childControlWidth = true;
            row.childControlHeight = true;

            for (int level = 1; level <= 5; level++)
            {
                CreateLevelButton(rowRect, level);
            }

            CreateText("Hint", panel.rectTransform, "CHOOSE A LEVEL TO START THE RUN", 14, MutedColor,
                new Vector2(0.08f, 0.1f), new Vector2(0.92f, 0.2f), TextAnchor.MiddleCenter);
            BindButtons();
        }

        private void CreateLevelButton(RectTransform parent, int level)
        {
            GameObject buttonObject = CreateObject($"Level{level}", parent);
            Image image = buttonObject.AddComponent<Image>();
            image.color = level == RunSelection.SelectedLevel ? AccentColor : ParseColor("243552");

            Button button = buttonObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.transition = Selectable.Transition.ColorTint;
            ColorBlock colors = button.colors;
            colors.normalColor = image.color;
            colors.highlightedColor = ParseColor("7DD3FC");
            colors.pressedColor = ParseColor("2DD4BF");
            colors.selectedColor = AccentColor;
            colors.disabledColor = ParseColor("334155");
            button.colors = colors;

            Text label = CreateText("Label", buttonObject.GetComponent<RectTransform>(),
                $"LEVEL {level}\n\nSTART", 18, TextColor,
                Vector2.zero, Vector2.one, TextAnchor.MiddleCenter);
            label.raycastTarget = false;
        }

        private void BindButtons()
        {
            Button[] buttons = GetComponentsInChildren<Button>(true);
            for (int i = 0; i < buttons.Length; i++)
            {
                Button button = buttons[i];
                if (!button.name.StartsWith("Level"))
                {
                    continue;
                }

                if (!int.TryParse(button.name.Substring("Level".Length), out int level))
                {
                    continue;
                }

                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => StartLevel(level));
            }
        }

        private void StartLevel(int level)
        {
            RunSelection.SelectLevel(level);

            if (BootScene.Instance == null)
            {
                SceneManager.LoadScene("GamePlayScene");
                return;
            }

            BootScene.Instance.LoadSceneAsync("GamePlayScene");
            BootScene.Instance.CloseLoadingScene();
        }

        private Canvas CreateCanvas()
        {
            GameObject canvasObject = CreateObject("HomeCanvas", transform);
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        private void EnsureEventSystem()
        {
            if (EventSystem.current != null)
            {
                return;
            }

            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        private Text CreateText(
            string objectName,
            Transform parent,
            string value,
            int size,
            Color color,
            Vector2 anchorMin,
            Vector2 anchorMax,
            TextAnchor alignment)
        {
            GameObject textObject = CreateObject(objectName, parent);
            RectTransform rect = textObject.GetComponent<RectTransform>();
            SetRect(rect, anchorMin, anchorMax, Vector2.zero, Vector2.zero);

            Text text = textObject.AddComponent<Text>();
            text.font = font;
            text.text = value;
            text.fontSize = size;
            text.color = color;
            text.alignment = alignment;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        private static Image CreateImage(string objectName, Transform parent, Color color)
        {
            GameObject imageObject = CreateObject(objectName, parent);
            Image image = imageObject.AddComponent<Image>();
            image.color = color;
            return image;
        }

        private static GameObject CreateObject(string objectName, Transform parent)
        {
            GameObject gameObject = new GameObject(objectName, typeof(RectTransform));
            gameObject.transform.SetParent(parent, false);
            return gameObject;
        }

        private static void Stretch(RectTransform rect)
        {
            SetRect(rect, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        }

        private static void SetRect(
            RectTransform rect,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 offsetMin,
            Vector2 offsetMax)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            rect.localScale = Vector3.one;
        }

        private static Color ParseColor(string html)
        {
            return ColorUtility.TryParseHtmlString($"#{html}", out Color color) ? color : Color.white;
        }
    }
}
