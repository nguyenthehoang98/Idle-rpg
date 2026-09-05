using System;
using System.Collections.Generic;
using _TDS.GameConfig;
using UnityEngine;
using UnityEngine.UI;

namespace _TDS.Gameplay
{
    public sealed class WaveUpgradePanel : MonoBehaviour
    {
        private static readonly Color PanelColor = ParseColor("172238");
        private static readonly Color CardColor = ParseColor("243552");
        private static readonly Color AccentColor = ParseColor("5EEAD4");
        private static readonly Color GoldColor = ParseColor("FDE047");
        private static readonly Color TextColor = ParseColor("F8FAFC");
        private static readonly Color MutedColor = ParseColor("A8B5C7");
        private static readonly Color DisabledColor = ParseColor("475569");

        private RectTransform safeArea;
        private GameObject overlay;
        private GameObject optionsRoot;
        private GameObject cardsRoot;
        private Text titleText;
        private Text subtitleText;
        private Text goldText;
        private Button backButton;
        private readonly Button[] cardButtons = new Button[3];
        private readonly Text[] cardTitles = new Text[3];
        private readonly Text[] cardDescriptions = new Text[3];
        private readonly Text[] cardCosts = new Text[3];
        private Action showShop;
        private Action showRoll;
        private Action showChoice;
        private Vector2Int lastScreenSize;
        private Font font;

        private void Awake()
        {
            BuildUi();
        }

        private void Update()
        {
            ApplySafeArea();
        }

        public void ShowChoice(int wave, int gold, Action shop, Action roll)
        {
            showShop = shop;
            showRoll = roll;
            showChoice = () => ShowChoice(wave, gold, shop, roll);
            overlay.SetActive(true);
            optionsRoot.SetActive(true);
            cardsRoot.SetActive(false);
            backButton.gameObject.SetActive(false);
            titleText.text = $"WAVE {wave} CLEARED";
            subtitleText.text = "CHOOSE ONE PATH FOR THE NEXT WAVE";
            goldText.text = $"GOLD  {gold}";
        }

        public void ShowCards(
            string title,
            string subtitle,
            int gold,
            IReadOnlyList<UpgradeCardConfigData> cards,
            bool requiresGold,
            Action<UpgradeCardConfigData> onSelect)
        {
            overlay.SetActive(true);
            optionsRoot.SetActive(false);
            cardsRoot.SetActive(true);
            backButton.gameObject.SetActive(true);
            titleText.text = title;
            subtitleText.text = subtitle;
            goldText.text = $"GOLD  {gold}";

            for (int i = 0; i < cardButtons.Length; i++)
            {
                bool visible = cards != null && i < cards.Count;
                cardButtons[i].gameObject.SetActive(visible);
                if (!visible) continue;

                UpgradeCardConfigData card = cards[i];
                bool affordable = !requiresGold || gold >= card.cost;
                cardTitles[i].text = card.title;
                cardDescriptions[i].text = card.description;
                cardCosts[i].text = requiresGold
                    ? affordable ? $"BUY  {card.cost} GOLD" : $"NEED  {card.cost} GOLD"
                    : "CHOOSE THIS UPGRADE";
                cardButtons[i].interactable = affordable;
                SetButtonColor(cardButtons[i], affordable ? CardColor : DisabledColor);
                cardButtons[i].onClick.RemoveAllListeners();
                cardButtons[i].onClick.AddListener(() => onSelect(card));
            }
        }

        public void Hide()
        {
            overlay.SetActive(false);
            showShop = null;
            showRoll = null;
            showChoice = null;
        }

        private void BuildUi()
        {
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            Canvas canvas = CreateCanvas();
            safeArea = canvas.transform.Find("SafeArea") as RectTransform;

            overlay = new GameObject("WaveUpgradeOverlay", typeof(RectTransform));
            overlay.transform.SetParent(safeArea, false);
            RectTransform overlayRect = overlay.GetComponent<RectTransform>();
            SetRect(overlayRect, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            Image scrim = CreateImage("Scrim", overlay.transform, new Color(0.02f, 0.05f, 0.1f, 0.9f));
            SetRect(scrim.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            Image panel = CreatePanel("UpgradePanel", overlay.transform, PanelColor);
            SetRect(panel.rectTransform, new Vector2(0.05f, 0.2f), new Vector2(0.95f, 0.8f), Vector2.zero, Vector2.zero);

            titleText = CreateText("Title", panel.transform, "WAVE CLEARED", 42, AccentColor,
                new Vector2(0.06f, 0.78f), new Vector2(0.72f, 0.94f), TextAnchor.MiddleLeft);
            subtitleText = CreateText("Subtitle", panel.transform, "CHOOSE ONE PATH", 20, MutedColor,
                new Vector2(0.06f, 0.69f), new Vector2(0.72f, 0.8f), TextAnchor.MiddleLeft);
            goldText = CreateText("Gold", panel.transform, "GOLD  0", 24, GoldColor,
                new Vector2(0.72f, 0.78f), new Vector2(0.94f, 0.92f), TextAnchor.MiddleRight);

            optionsRoot = new GameObject("Options", typeof(RectTransform));
            optionsRoot.transform.SetParent(panel.transform, false);
            SetRect(optionsRoot.GetComponent<RectTransform>(), new Vector2(0.06f, 0.24f), new Vector2(0.94f, 0.64f), Vector2.zero, Vector2.zero);

            Button shopButton = CreateButton("GoldShop", optionsRoot.transform, "GOLD SHOP\n3 ITEMS", AccentColor,
                new Vector2(0.02f, 0f), new Vector2(0.47f, 1f));
            shopButton.onClick.AddListener(() => showShop?.Invoke());
            Button rollButton = CreateButton("UpgradeRoll", optionsRoot.transform, "UPGRADE ROLL\n3 CARDS → 1", GoldColor,
                new Vector2(0.53f, 0f), new Vector2(0.98f, 1f));
            rollButton.onClick.AddListener(() => showRoll?.Invoke());

            cardsRoot = new GameObject("Cards", typeof(RectTransform));
            cardsRoot.transform.SetParent(panel.transform, false);
            SetRect(cardsRoot.GetComponent<RectTransform>(), new Vector2(0.04f, 0.2f), new Vector2(0.96f, 0.68f), Vector2.zero, Vector2.zero);

            for (int i = 0; i < cardButtons.Length; i++)
            {
                float min = 0.01f + i * 0.33f;
                float max = min + 0.31f;
                Button button = CreateButton($"Card{i}", cardsRoot.transform, "", AccentColor,
                    new Vector2(min, 0.02f), new Vector2(max, 0.98f));
                cardButtons[i] = button;
                cardTitles[i] = CreateText("CardTitle", button.transform, "UPGRADE", 24, TextColor,
                    new Vector2(0.08f, 0.65f), new Vector2(0.92f, 0.93f), TextAnchor.MiddleCenter);
                cardDescriptions[i] = CreateText("CardDescription", button.transform, "", 18, MutedColor,
                    new Vector2(0.1f, 0.28f), new Vector2(0.9f, 0.66f), TextAnchor.MiddleCenter);
                cardCosts[i] = CreateText("CardCost", button.transform, "CHOOSE THIS UPGRADE", 16, AccentColor,
                    new Vector2(0.08f, 0.06f), new Vector2(0.92f, 0.24f), TextAnchor.MiddleCenter);
                cardTitles[i].raycastTarget = false;
                cardDescriptions[i].raycastTarget = false;
                cardCosts[i].raycastTarget = false;
            }

            backButton = CreateButton("Back", panel.transform, "BACK", MutedColor,
                new Vector2(0.06f, 0.07f), new Vector2(0.22f, 0.16f));
            backButton.onClick.AddListener(() => showChoice?.Invoke());
            overlay.SetActive(false);
        }

        private Canvas CreateCanvas()
        {
            GameObject canvasObject = new GameObject("WaveUpgradeCanvas", typeof(RectTransform));
            canvasObject.transform.SetParent(transform, false);
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 70;

            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 2160f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>();

            GameObject safeAreaObject = new GameObject("SafeArea", typeof(RectTransform));
            safeAreaObject.transform.SetParent(canvasObject.transform, false);
            SetRect(safeAreaObject.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            return canvas;
        }

        private void ApplySafeArea()
        {
            if (safeArea == null || Screen.width <= 0 || Screen.height <= 0) return;
            Vector2Int screenSize = new Vector2Int(Screen.width, Screen.height);
            if (screenSize == lastScreenSize) return;
            lastScreenSize = screenSize;

            Rect area = Screen.safeArea;
            safeArea.anchorMin = new Vector2(area.xMin / Screen.width, area.yMin / Screen.height);
            safeArea.anchorMax = new Vector2(area.xMax / Screen.width, area.yMax / Screen.height);
            safeArea.offsetMin = Vector2.zero;
            safeArea.offsetMax = Vector2.zero;
        }

        private Button CreateButton(
            string objectName,
            Transform parent,
            string label,
            Color accent,
            Vector2 anchorMin,
            Vector2 anchorMax)
        {
            GameObject buttonObject = new GameObject(objectName, typeof(RectTransform));
            buttonObject.transform.SetParent(parent, false);
            SetRect(buttonObject.GetComponent<RectTransform>(), anchorMin, anchorMax, Vector2.zero, Vector2.zero);

            Image image = buttonObject.AddComponent<Image>();
            image.color = CardColor;
            AddOutline(image, new Color(accent.r, accent.g, accent.b, 0.45f), 2f);
            Button button = buttonObject.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = CardColor;
            colors.highlightedColor = new Color(accent.r, accent.g, accent.b, 0.2f);
            colors.pressedColor = new Color(accent.r, accent.g, accent.b, 0.35f);
            colors.disabledColor = DisabledColor;
            button.colors = colors;

            Text text = CreateText("Label", buttonObject.transform, label, 26, TextColor,
                new Vector2(0.08f, 0.12f), new Vector2(0.92f, 0.88f), TextAnchor.MiddleCenter);
            text.raycastTarget = false;
            return button;
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
            SetRect(textObject.GetComponent<RectTransform>(), anchorMin, anchorMax, Vector2.zero, Vector2.zero);
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

        private static Image CreatePanel(string objectName, Transform parent, Color color)
        {
            Image panel = CreateImage(objectName, parent, color);
            AddOutline(panel, new Color(0.37f, 0.91f, 0.83f, 0.2f), 2f);
            return panel;
        }

        private static void AddOutline(Image image, Color color, float distance)
        {
            Outline outline = image.gameObject.AddComponent<Outline>();
            outline.effectColor = color;
            outline.effectDistance = new Vector2(distance, distance);
            outline.useGraphicAlpha = true;
        }

        private static void SetButtonColor(Button button, Color color)
        {
            ColorBlock colors = button.colors;
            colors.normalColor = color;
            colors.highlightedColor = color;
            button.colors = colors;
        }

        private static void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
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
