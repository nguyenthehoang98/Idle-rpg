using _TDS.Battle;
using UnityEngine;
using UnityEngine.UI;

namespace _TDS.Gameplay
{
    public sealed class GameplayHud : MonoBehaviour
    {
        private static readonly Color BackgroundColor = ParseColor("0B1220");
        private static readonly Color PanelColor = ParseColor("172238");
        private static readonly Color SlotColor = ParseColor("243552");
        private static readonly Color EmptyColor = ParseColor("334155");
        private static readonly Color AccentColor = ParseColor("5EEAD4");
        private static readonly Color TextColor = ParseColor("F8FAFC");
        private static readonly Color MutedColor = ParseColor("A8B5C7");

        private readonly Image[] slotImages = new Image[EnergyCircuit.DefaultSlotCount];
        private readonly Text[] slotLabels = new Text[EnergyCircuit.DefaultSlotCount];
        private readonly float[] slotFeedback = new float[EnergyCircuit.DefaultSlotCount];

        private Font font;
        private Text waveText;
        private Text statusText;

        private void Awake()
        {
            BuildUi();
        }

        private void Update()
        {
            for (int i = 0; i < slotFeedback.Length; i++)
            {
                if (slotFeedback[i] <= 0f)
                {
                    continue;
                }

                slotFeedback[i] -= Time.unscaledDeltaTime;
                if (slotFeedback[i] <= 0f)
                {
                    slotImages[i].color = SlotColor;
                }
            }
        }

        public void Initialize(CircuitBoard board, int level)
        {
            if (board == null)
            {
                return;
            }

            SetLevel(level);
            for (int i = 0; i < slotImages.Length && i < board.SlotCount; i++)
            {
                CircuitSlotContent content = board.GetContent(i);
                slotLabels[i].text = content.Type switch
                {
                    CircuitSlotContentType.Hero => $"H\n{content.Id}",
                    CircuitSlotContentType.Item => $"I\n{content.Id}",
                    _ => "·"
                };
                slotImages[i].color = content.Type == CircuitSlotContentType.Empty ? EmptyColor : SlotColor;
            }
        }

        public void SetLevel(int level)
        {
            SetText("LEVEL " + level, waveText);
        }

        public void SetWave(int wave)
        {
            SetText("WAVE " + wave, waveText);
        }

        public void SetStatus(string status)
        {
            SetText(status, statusText);
        }

        public void ActivateSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= slotImages.Length)
            {
                return;
            }

            slotImages[slotIndex].color = AccentColor;
            slotFeedback[slotIndex] = 0.65f;
        }

        private void BuildUi()
        {
            font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            Canvas canvas = CreateCanvas();
            RectTransform root = canvas.GetComponent<RectTransform>();

            Image topBar = CreateImage("TopBar", root, PanelColor);
            SetRect(topBar.rectTransform, new Vector2(0f, 0.86f), Vector2.one, Vector2.zero, Vector2.zero);

            CreateText("Title", topBar.transform, "IDLE // CIRCUIT", 20, TextColor,
                new Vector2(0.03f, 0f), new Vector2(0.35f, 1f), TextAnchor.MiddleLeft);
            waveText = CreateText("Wave", topBar.transform, "LEVEL 1", 18, AccentColor,
                new Vector2(0.4f, 0f), new Vector2(0.6f, 1f), TextAnchor.MiddleCenter);
            statusText = CreateText("Status", topBar.transform, "AUTO COMBAT", 14, MutedColor,
                new Vector2(0.65f, 0f), new Vector2(0.97f, 1f), TextAnchor.MiddleRight);

            Image circuitPanel = CreateImage("CircuitPanel", root, BackgroundColor);
            SetRect(circuitPanel.rectTransform, new Vector2(0.04f, 0.03f), new Vector2(0.96f, 0.19f), Vector2.zero, Vector2.zero);
            CreateText("CircuitTitle", circuitPanel.transform, "ENERGY CIRCUIT", 12, MutedColor,
                new Vector2(0.02f, 0.68f), new Vector2(0.2f, 0.98f), TextAnchor.MiddleLeft);

            GameObject slotRowObject = new GameObject("Slots", typeof(RectTransform));
            slotRowObject.transform.SetParent(circuitPanel.transform, false);
            RectTransform slotRowRect = slotRowObject.GetComponent<RectTransform>();
            SetRect(slotRowRect, new Vector2(0.2f, 0.12f), new Vector2(0.98f, 0.82f), Vector2.zero, Vector2.zero);
            HorizontalLayoutGroup slotRow = slotRowObject.AddComponent<HorizontalLayoutGroup>();
            slotRow.spacing = 8f;
            slotRow.padding = new RectOffset(4, 4, 4, 4);
            slotRow.childControlWidth = true;
            slotRow.childControlHeight = true;
            slotRow.childForceExpandWidth = true;
            slotRow.childForceExpandHeight = true;

            for (int i = 0; i < slotImages.Length; i++)
            {
                GameObject slotObject = new GameObject($"Slot{i}", typeof(RectTransform));
                slotObject.transform.SetParent(slotRowObject.transform, false);
                Image image = slotObject.AddComponent<Image>();
                image.color = EmptyColor;
                slotImages[i] = image;
                slotLabels[i] = CreateText("Label", slotObject.transform, "·", 12, TextColor,
                    Vector2.zero, Vector2.one, TextAnchor.MiddleCenter);
                slotLabels[i].raycastTarget = false;
            }
        }

        private Canvas CreateCanvas()
        {
            GameObject canvasObject = new GameObject("GameplayCanvas", typeof(RectTransform));
            canvasObject.transform.SetParent(transform, false);
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;

            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>();
            return canvas;
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
            GameObject textObject = new GameObject(objectName, typeof(RectTransform));
            textObject.transform.SetParent(parent, false);
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
            GameObject imageObject = new GameObject(objectName, typeof(RectTransform));
            imageObject.transform.SetParent(parent, false);
            Image image = imageObject.AddComponent<Image>();
            image.color = color;
            return image;
        }

        private static void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            rect.localScale = Vector3.one;
        }

        private static void SetText(string value, Text text)
        {
            if (text != null)
            {
                text.text = value;
            }
        }

        private static Color ParseColor(string html)
        {
            return ColorUtility.TryParseHtmlString($"#{html}", out Color color) ? color : Color.white;
        }
    }
}
