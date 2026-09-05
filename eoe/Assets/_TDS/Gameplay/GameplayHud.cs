using _TDS.Battle;
using _TDS.GameConfig;
using UnityEngine;
using UnityEngine.UI;

namespace _TDS.Gameplay
{
    public sealed class GameplayHud : MonoBehaviour
    {
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
        private RectTransform safeArea;
        private Text waveText;
        private Text statusText;
        private Text resultTitle;
        private Text resultRewards;
        private GameObject resultOverlay;
        private float modifierFeedbackRemaining;
        private string statusBeforeModifier;
        private Color statusBeforeModifierColor;
        private Vector2Int lastScreenSize;

        private void Awake()
        {
            BuildUi();
        }

        private void Update()
        {
            ApplySafeArea();

            if (modifierFeedbackRemaining > 0f)
            {
                modifierFeedbackRemaining -= Time.unscaledDeltaTime;
                if (modifierFeedbackRemaining <= 0f)
                {
                    SetText(statusBeforeModifier, statusText);
                    if (statusText != null)
                    {
                        statusText.color = statusBeforeModifierColor;
                    }
                }
            }

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
            if (modifierFeedbackRemaining > 0f)
            {
                statusBeforeModifier = status;
                return;
            }

            SetText(status, statusText);
            if (statusText != null)
            {
                statusText.color = MutedColor;
            }
        }

        public void ShowModifierFeedback(SkillModifierType type)
        {
            if (statusText == null)
            {
                return;
            }

            if (modifierFeedbackRemaining <= 0f)
            {
                statusBeforeModifier = statusText.text;
                statusBeforeModifierColor = statusText.color;
            }

            statusText.text = $"STATUS: {type.ToString().ToUpperInvariant()}";
            statusText.color = ModifierColor(type);
            modifierFeedbackRemaining = 1f;
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

        public void ShowResult(bool victory, BattleRunRewards rewards)
        {
            if (resultOverlay == null || rewards == null)
            {
                return;
            }

            resultOverlay.SetActive(true);
            resultTitle.text = victory ? "VICTORY" : "DEFEAT";
            resultTitle.color = victory ? AccentColor : ParseColor("FB7185");
            resultRewards.text = $"{rewards.DefeatedMonsters} MONSTERS DEFEATED\n+{rewards.Experience} EXP   +{rewards.Gold} GOLD";
        }

        private void BuildUi()
        {
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            Canvas canvas = CreateCanvas();
            RectTransform root = safeArea;

            Image topBar = CreatePanel("TopBar", root, PanelColor);
            SetRect(topBar.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(24f, -174f), new Vector2(-24f, -24f));

            Image topAccent = CreateImage("Accent", topBar.transform, AccentColor);
            SetRect(topAccent.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 1f),
                Vector2.zero, new Vector2(6f, 0f));

            CreateText("Title", topBar.transform, "IDLE // CIRCUIT", 32, TextColor,
                new Vector2(0.06f, 0f), new Vector2(0.43f, 1f), TextAnchor.MiddleLeft);
            waveText = CreateText("Wave", topBar.transform, "LEVEL 1", 28, AccentColor,
                new Vector2(0.43f, 0f), new Vector2(0.65f, 1f), TextAnchor.MiddleCenter);
            statusText = CreateText("Status", topBar.transform, "AUTO COMBAT", 22, MutedColor,
                new Vector2(0.65f, 0f), new Vector2(0.94f, 1f), TextAnchor.MiddleRight);

            Image circuitPanel = CreatePanel("CircuitPanel", root, PanelColor);
            SetRect(circuitPanel.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f),
                new Vector2(24f, 36f), new Vector2(-24f, 390f));

            CreateText("CircuitTitle", circuitPanel.transform, "ENERGY CIRCUIT", 24, TextColor,
                new Vector2(0.04f, 0.79f), new Vector2(0.5f, 0.98f), TextAnchor.MiddleLeft);
            CreateText("CircuitHint", circuitPanel.transform, "PULSE  ·  3 STACKS TO OVERDRIVE", 16, MutedColor,
                new Vector2(0.04f, 0.64f), new Vector2(0.65f, 0.81f), TextAnchor.MiddleLeft);

            Image circuitRule = CreateImage("Rule", circuitPanel.transform, AccentColor);
            circuitRule.color = new Color(AccentColor.r, AccentColor.g, AccentColor.b, 0.35f);
            SetRect(circuitRule.rectTransform, new Vector2(0.04f, 0.6f), new Vector2(0.96f, 0.6f),
                Vector2.zero, new Vector2(0f, 2f));

            GameObject slotRowObject = new GameObject("Slots", typeof(RectTransform));
            slotRowObject.transform.SetParent(circuitPanel.transform, false);
            RectTransform slotRowRect = slotRowObject.GetComponent<RectTransform>();
            SetRect(slotRowRect, new Vector2(0.03f, 0.08f), new Vector2(0.97f, 0.57f),
                Vector2.zero, Vector2.zero);
            HorizontalLayoutGroup slotRow = slotRowObject.AddComponent<HorizontalLayoutGroup>();
            slotRow.spacing = 12f;
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
                AddOutline(image, new Color(0.37f, 0.91f, 0.83f, 0.22f), 2f);
                slotImages[i] = image;
                slotLabels[i] = CreateText("Label", slotObject.transform, "·", 22, TextColor,
                    Vector2.zero, Vector2.one, TextAnchor.MiddleCenter);
                slotLabels[i].raycastTarget = false;
            }

            BuildResultOverlay(root);
        }

        private void BuildResultOverlay(Transform root)
        {
            resultOverlay = new GameObject("ResultOverlay", typeof(RectTransform));
            resultOverlay.transform.SetParent(root, false);
            RectTransform overlayRect = resultOverlay.GetComponent<RectTransform>();
            SetRect(overlayRect, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            Image scrim = CreateImage("Scrim", resultOverlay.transform, new Color(0.02f, 0.05f, 0.1f, 0.82f));
            SetRect(scrim.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            Image card = CreatePanel("ResultCard", resultOverlay.transform, PanelColor);
            SetRect(card.rectTransform, new Vector2(0.1f, 0.39f), new Vector2(0.9f, 0.61f),
                Vector2.zero, Vector2.zero);

            resultTitle = CreateText("ResultTitle", card.transform, "RESULT", 44, AccentColor,
                new Vector2(0.08f, 0.5f), new Vector2(0.92f, 0.84f), TextAnchor.MiddleCenter);
            resultRewards = CreateText("ResultRewards", card.transform, "", 22, TextColor,
                new Vector2(0.08f, 0.14f), new Vector2(0.92f, 0.52f), TextAnchor.MiddleCenter);
            resultOverlay.SetActive(false);
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
            scaler.referenceResolution = new Vector2(1080f, 2160f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>();

            GameObject safeAreaObject = new GameObject("SafeArea", typeof(RectTransform));
            safeAreaObject.transform.SetParent(canvasObject.transform, false);
            safeArea = safeAreaObject.GetComponent<RectTransform>();
            ApplySafeArea();
            return canvas;
        }

        private void ApplySafeArea()
        {
            if (safeArea == null || Screen.width <= 0 || Screen.height <= 0)
            {
                return;
            }

            Vector2Int screenSize = new Vector2Int(Screen.width, Screen.height);
            if (screenSize == lastScreenSize)
            {
                return;
            }

            lastScreenSize = screenSize;
            Rect area = Screen.safeArea;
            safeArea.anchorMin = new Vector2(area.xMin / Screen.width, area.yMin / Screen.height);
            safeArea.anchorMax = new Vector2(area.xMax / Screen.width, area.yMax / Screen.height);
            safeArea.offsetMin = Vector2.zero;
            safeArea.offsetMax = Vector2.zero;
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

        private static Image CreatePanel(string objectName, Transform parent, Color color)
        {
            Image panel = CreateImage(objectName, parent, color);
            AddOutline(panel, new Color(0.37f, 0.91f, 0.83f, 0.16f), 2f);
            return panel;
        }

        private static void AddOutline(Image image, Color color, float distance)
        {
            Outline outline = image.gameObject.AddComponent<Outline>();
            outline.effectColor = color;
            outline.effectDistance = new Vector2(distance, distance);
            outline.useGraphicAlpha = true;
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

        private static Color ModifierColor(SkillModifierType type)
        {
            return type switch
            {
                SkillModifierType.Slow => ParseColor("FBBF24"),
                SkillModifierType.Bleed => ParseColor("FB7185"),
                SkillModifierType.Silence => ParseColor("C084FC"),
                SkillModifierType.Stun => ParseColor("F97316"),
                _ => AccentColor,
            };
        }

        private static Color ParseColor(string html)
        {
            return ColorUtility.TryParseHtmlString($"#{html}", out Color color) ? color : Color.white;
        }
    }
}
