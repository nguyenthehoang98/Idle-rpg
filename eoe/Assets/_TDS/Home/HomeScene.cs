using System.Collections.Generic;
using Newtonsoft.Json;
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
            Transform existingCanvas = transform.Find("HomeCanvas");
            if (existingCanvas != null)
            {
                if (!Application.isPlaying)
                {
                    BindButtons();
                    return;
                }

                existingCanvas.gameObject.SetActive(false);
                Destroy(existingCanvas.gameObject);
            }

            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            HomeUiSpec spec = LoadSpec();
            EnsureEventSystem();

            Canvas canvas = CreateCanvas();
            RectTransform root = canvas.GetComponent<RectTransform>();

            Image background = CreateImage("Background", root, BackgroundColor);
            Stretch(background.rectTransform);

            FigmaNode importedImage = FindNodeByType(LoadHierarchyRoot(), "RECTANGLE");
            if (importedImage != null)
            {
                CreateFigmaImage(root, importedImage);
            }

            Image panel = CreateImage("Panel", root, PanelColor);
            SetRect(panel.rectTransform, new Vector2(0.08f, 0.1f), new Vector2(0.92f, 0.9f), Vector2.zero, Vector2.zero);

            CreateText("Title", panel.rectTransform, spec.title, spec.titleFontSize, TextColor,
                new Vector2(0.08f, 0.78f), new Vector2(0.92f, 0.94f), TextAnchor.MiddleCenter);
            CreateText("Subtitle", panel.rectTransform, spec.subtitle, spec.subtitleFontSize, MutedColor,
                new Vector2(0.08f, 0.68f), new Vector2(0.92f, 0.78f), TextAnchor.MiddleCenter);

            GameObject levelRow = CreateObject("LevelRow", panel.rectTransform);
            RectTransform rowRect = levelRow.GetComponent<RectTransform>();
            SetRect(rowRect, new Vector2(0.06f, 0.26f), new Vector2(0.94f, 0.66f), Vector2.zero, Vector2.zero);
            HorizontalLayoutGroup row = levelRow.AddComponent<HorizontalLayoutGroup>();
            row.spacing = 12f;
            row.padding = new RectOffset(4, 4, 4, 4);
            row.childForceExpandWidth = false;
            row.childForceExpandHeight = false;
            row.childControlWidth = true;
            row.childControlHeight = true;

            for (int i = 0; i < spec.buttons.Length; i++)
            {
                CreateLevelButton(rowRect, spec.buttons[i]);
            }

            CreateText("Hint", panel.rectTransform, spec.hint, spec.hintFontSize, MutedColor,
                new Vector2(0.08f, 0.1f), new Vector2(0.92f, 0.2f), TextAnchor.MiddleCenter);
            BindButtons();
        }

        private void CreateLevelButton(RectTransform parent, HomeUiButtonSpec spec)
        {
            GameObject buttonObject = CreateObject($"Level{spec.level}", parent);
            Image image = buttonObject.AddComponent<Image>();
            image.color = spec.level == RunSelection.SelectedLevel ? AccentColor : ParseColor("243552");

            LayoutElement layout = buttonObject.AddComponent<LayoutElement>();
            layout.minWidth = spec.width;
            layout.minHeight = spec.height;
            layout.preferredWidth = spec.width;
            layout.preferredHeight = spec.height;
            layout.flexibleWidth = 0f;
            layout.flexibleHeight = 0f;

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
                spec.label, spec.labelFontSize, TextColor,
                Vector2.zero, Vector2.one, TextAnchor.MiddleCenter);
            label.raycastTarget = false;
        }

        private HomeUiSpec LoadSpec()
        {
            HomeUiSpec hierarchySpec = LoadHierarchySpec();
            if (hierarchySpec != null)
            {
                return hierarchySpec;
            }

            TextAsset asset = Resources.Load<TextAsset>("UI/ui-spec");
            if (asset != null)
            {
                HomeUiSpec spec = JsonUtility.FromJson<HomeUiSpec>(asset.text);
                if (spec != null && spec.buttons != null && spec.buttons.Length > 0)
                {
                    return spec;
                }
            }

            return HomeUiSpec.Default();
        }

        private static FigmaNode LoadHierarchyRoot()
        {
            TextAsset asset = Resources.Load<TextAsset>("UI/hierarchy");
            return asset == null ? null : JsonConvert.DeserializeObject<FigmaNode>(asset.text);
        }

        private static HomeUiSpec LoadHierarchySpec()
        {
            TextAsset asset = Resources.Load<TextAsset>("UI/hierarchy");
            if (asset == null)
            {
                return null;
            }

            try
            {
                FigmaNode root = JsonConvert.DeserializeObject<FigmaNode>(asset.text);
                if (root == null)
                {
                    return null;
                }

                HomeUiSpec spec = HomeUiSpec.Default();
                spec.title = ReadText(FindNode(root, "Title"), "Title", spec.title, out spec.titleFontSize);
                spec.subtitle = ReadText(FindNode(root, "Subtitle"), "Subtitle", spec.subtitle, out spec.subtitleFontSize);
                spec.hint = ReadText(FindNode(root, "Hint"), "Hint", spec.hint, out spec.hintFontSize);

                FigmaNode grid = FindNode(root, "LevelGrid");
                if (grid?.children == null)
                {
                    return spec;
                }

                List<HomeUiButtonSpec> buttons = new List<HomeUiButtonSpec>();
                for (int i = 0; i < grid.children.Length; i++)
                {
                    FigmaNode levelNode = grid.children[i];
                    if (!levelNode.name.StartsWith("Level") ||
                        !int.TryParse(levelNode.name.Substring("Level".Length), out int level))
                    {
                        continue;
                    }

                    FigmaNode labelNode = FindNode(levelNode, "Label");
                    string label = ReadText(labelNode, "Label", $"LEVEL {level}\\n\\nSTART", out int labelFontSize);
                    FigmaComponent rect = FindComponent(levelNode, "RectTransform");
                    buttons.Add(new HomeUiButtonSpec
                    {
                        level = level,
                        label = label,
                        width = rect?.width > 0f ? rect.width : 148f,
                        height = rect?.height > 0f ? rect.height : 180f,
                        labelFontSize = labelFontSize,
                    });
                }

                if (buttons.Count > 0)
                {
                    spec.buttons = buttons.ToArray();
                }

                return spec;
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning($"Unable to load Figma hierarchy: {exception.Message}");
                return null;
            }
        }

        private static string ReadText(
            FigmaNode node,
            string _name,
            string fallback,
            out int fontSize)
        {
            fontSize = 18;
            FigmaComponent text = FindComponent(node, "Text");
            if (text == null)
            {
                return fallback;
            }

            fontSize = Mathf.Max(1, Mathf.RoundToInt(text.fontSize));
            return string.IsNullOrEmpty(text.text) ? fallback : text.text;
        }

        private static FigmaNode FindNodeByType(FigmaNode node, string type)
        {
            if (node == null)
            {
                return null;
            }
            if (node.type == type && FindComponent(node, "Image") != null)
            {
                return node;
            }
            if (node.children == null)
            {
                return null;
            }

            for (int i = 0; i < node.children.Length; i++)
            {
                FigmaNode match = FindNodeByType(node.children[i], type);
                if (match != null)
                {
                    return match;
                }
            }
            return null;
        }

        private static FigmaNode FindNode(FigmaNode node, string name)
        {
            if (node == null)
            {
                return null;
            }
            if (node.name == name)
            {
                return node;
            }
            if (node.children == null)
            {
                return null;
            }

            for (int i = 0; i < node.children.Length; i++)
            {
                FigmaNode match = FindNode(node.children[i], name);
                if (match != null)
                {
                    return match;
                }
            }
            return null;
        }

        private static void CreateFigmaImage(RectTransform parent, FigmaNode node)
        {
            FigmaComponent rect = FindComponent(node, "RectTransform");
            FigmaComponent imageData = FindComponent(node, "Image");
            if (rect == null || imageData == null || string.IsNullOrEmpty(imageData.sourceImage))
            {
                return;
            }

            string resourcePath = $"UI/FigmaExport/Images/{System.IO.Path.GetFileNameWithoutExtension(imageData.sourceImage)}";
            Sprite sprite = Resources.Load<Sprite>(resourcePath);
            if (sprite == null)
            {
                Sprite[] sprites = Resources.LoadAll<Sprite>(resourcePath);
                sprite = sprites.Length > 0 ? sprites[0] : null;
            }
            if (sprite == null)
            {
                Debug.LogWarning($"Figma image not found in Resources: {resourcePath}");
                return;
            }

            GameObject imageObject = CreateObject(node.name, parent);
            RectTransform imageRect = imageObject.GetComponent<RectTransform>();
            imageRect.anchorMin = new Vector2(0.5f, 0.5f);
            imageRect.anchorMax = new Vector2(0.5f, 0.5f);
            imageRect.pivot = new Vector2(0.5f, 0.5f);
            imageRect.sizeDelta = new Vector2(rect.width, rect.height);
            imageRect.anchoredPosition = new Vector2(rect.x, rect.y);

            Image image = imageObject.AddComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = true;
            image.color = imageData.color == null
                ? Color.white
                : new Color(imageData.color.r, imageData.color.g, imageData.color.b, imageData.color.a);
        }

        private static FigmaComponent FindComponent(FigmaNode node, string name)
        {
            if (node?.components == null)
            {
                return null;
            }

            for (int i = 0; i < node.components.Length; i++)
            {
                if (node.components[i].component == name)
                {
                    return node.components[i];
                }
            }
            return null;
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

    [System.Serializable]
    public sealed class HomeUiSpec
    {
        public string screen;
        public string title;
        public string subtitle;
        public string hint;
        public int titleFontSize = 34;
        public int subtitleFontSize = 16;
        public int hintFontSize = 14;
        public HomeUiButtonSpec[] buttons;

        public static HomeUiSpec Default()
        {
            HomeUiSpec spec = new HomeUiSpec
            {
                screen = "home",
                title = "IDLE // CIRCUIT",
                subtitle = "SELECT A LEVEL",
                hint = "CHOOSE A LEVEL TO START THE RUN",
                buttons = new HomeUiButtonSpec[5],
            };

            for (int i = 0; i < spec.buttons.Length; i++)
            {
                int level = i + 1;
                spec.buttons[i] = new HomeUiButtonSpec
                {
                    level = level,
                    label = $"LEVEL {level}\n\nSTART",
                    width = 148f,
                    height = 180f,
                    labelFontSize = 18,
                };
            }

            return spec;
        }
    }

    [System.Serializable]
    public sealed class HomeUiButtonSpec
    {
        public int level;
        public string label;
        public float width = 148f;
        public float height = 180f;
        public int labelFontSize = 18;
    }

    [System.Serializable]
    sealed class FigmaNode
    {
        public string name;
        public string type;
        public FigmaNode[] children;
        public FigmaComponent[] components;
    }

    [System.Serializable]
    sealed class FigmaComponent
    {
        public string component;
        public float width;
        public float height;
        public float x;
        public float y;
        public string text;
        public float fontSize;
        public string sourceImage;
        public FigmaColor color;
    }

    [System.Serializable]
    sealed class FigmaColor
    {
        public float r;
        public float g;
        public float b;
        public float a = 1f;
    }
}
