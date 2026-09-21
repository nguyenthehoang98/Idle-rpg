using System;
using _TDS.Battle;
using _TDS.GameConfig;
using UnityEngine;
using UnityEngine.UI;

namespace _TDS.Gameplay
{
    public sealed class GameplayHud : MonoBehaviour
    {
        private static readonly Color SlotColor = ParseColor("FFFFFF");
        private static readonly Color AccentColor = ParseColor("5EEAD4");
        private static readonly Color HighlightColor = ParseColor("FDE047");
        private static readonly Color TextColor = ParseColor("F8FAFC");
        private static readonly Color MutedColor = ParseColor("A8B5C7");

        [SerializeField] private Canvas gameplayCanvas;

        private readonly Image[] slotImages = new Image[EnergyCircuit.DefaultSlotCount];
        private readonly Text[] slotLabels = new Text[EnergyCircuit.DefaultSlotCount];
        private readonly Text[] slotIndexLabels = new Text[EnergyCircuit.DefaultSlotCount];
        private readonly Text[] slotStackLabels = new Text[EnergyCircuit.DefaultSlotCount];
        private readonly Color[] slotBaseColors = new Color[EnergyCircuit.DefaultSlotCount];
        private readonly float[] slotFeedback = new float[EnergyCircuit.DefaultSlotCount];

        private RectTransform safeArea;
        private GameObject bottomPanel;
        private Text waveText;
        private Text statusText;
        private Text goldText;
        private Text energyText;
        private Text rollResultText;
        private GameObject rollPopup;
        private Text rollPopupText;
        private float rollPopupRemaining;
        private Text modeButtonLabel;
        private Button rollButton;
        private Button modeButton;
        private Action<bool> setManualMode;
        private Func<bool> rollRequested;
        private bool manualMode;
        private float rollResultRemaining;
        private Text resultTitle;
        private Text resultRewards;
        private GameObject resultOverlay;
        private Action<float> setTimeScale;
        private Action<bool> continueRun;
        private Action returnHome;
        private readonly Button[] speedButtons = new Button[3];
        private Button resultContinueButton;
        private Text resultContinueLabel;
        private float modifierFeedbackRemaining;
        private string statusBeforeModifier;
        private Color statusBeforeModifierColor;

        private void Awake()
        {
            if (!BindTemplate())
            {
                enabled = false;
            }
        }

        private void Update()
        {
            if (modifierFeedbackRemaining > 0f)
            {
                modifierFeedbackRemaining -= Time.unscaledDeltaTime;
                if (modifierFeedbackRemaining <= 0f)
                {
                    SetText(statusBeforeModifier, statusText);
                    if (statusText != null) statusText.color = statusBeforeModifierColor;
                }
            }

            if (rollResultRemaining > 0f)
            {
                rollResultRemaining -= Time.unscaledDeltaTime;
                if (rollResultRemaining <= 0f) SetText(string.Empty, rollResultText);
            }

            if (rollPopupRemaining > 0f)
            {
                rollPopupRemaining -= Time.unscaledDeltaTime;
                if (rollPopupRemaining <= 0f && rollPopup != null) rollPopup.SetActive(false);
            }

            for (int i = 0; i < slotFeedback.Length; i++)
            {
                if (slotFeedback[i] <= 0f) continue;
                slotFeedback[i] -= Time.unscaledDeltaTime;
                if (slotFeedback[i] <= 0f && slotImages[i] != null) slotImages[i].color = slotBaseColors[i];
            }
        }

        public void BindTimeScale(Action<float> setter)
        {
            setTimeScale = setter;
            SetSpeed(1f);
        }

        public void BindResultActions(Action<bool> onContinue, Action onHome)
        {
            continueRun = onContinue;
            returnHome = onHome;
        }

        public void Initialize(CircuitBoard board, int level)
        {
            if (board == null) return;

            SetLevel(level);
            for (int i = 0; i < slotImages.Length && i < board.SlotCount; i++)
            {
                CircuitSlotContent content = board.GetContent(i);
                SetText(content.Type switch
                {
                    CircuitSlotContentType.Hero => $"HERO\n{content.Id}",
                    CircuitSlotContentType.Item => $"ITEM\n{content.Id}",
                    _ => "EMPTY"
                }, slotLabels[i]);
                SetText($"{i + 1:00}", slotIndexLabels[i]);
                SetText(string.Empty, slotStackLabels[i]);
                slotBaseColors[i] = SlotColor;
                if (slotImages[i] != null)
                {
                    slotImages[i].color = SlotColor;
                    slotImages[i].gameObject.SetActive(true);
                }
            }
        }

        public void SetLevel(int level) => SetText("LEVEL " + level, waveText);

        public void SetWave(int wave) => SetText("WAVE " + wave, waveText);

        public void SetGold(int gold) => SetText($"GOLD  {Mathf.Max(0, gold)}", goldText);

        public void BindCircuitControls(Func<bool> roll, Action<bool> manualModeSetter)
        {
            rollRequested = roll;
            setManualMode = manualModeSetter;
            SetManualMode(false, notify: false);
        }

        public void ShowRoll(CircuitRollEvent roll)
        {
            SetText($"ROLL +{roll.StepsMoved}  →  {roll.SlotIndex + 1:00}", rollResultText);
            rollResultRemaining = 1.5f;
            if (rollPopup != null)
            {
                SetText($"+{roll.StepsMoved}", rollPopupText);
                rollPopup.SetActive(true);
                rollPopupRemaining = 1f;
            }
        }

        public void RefreshCircuit(EnergyCircuit circuit)
        {
            if (circuit == null) return;

            SetText($"ENERGY  {Mathf.FloorToInt(circuit.Energy)}/{Mathf.FloorToInt(circuit.EnergyCapacity)}", energyText);
            if (rollButton != null) rollButton.interactable = manualMode && circuit.IsReady;

            for (int i = 0; i < slotImages.Length; i++)
            {
                if (slotImages[i] == null) continue;
                bool visible = i < circuit.SlotCount;
                slotImages[i].gameObject.SetActive(visible);
                if (!visible) continue;
                CircuitSlotState state = circuit.GetSlot(i);
                SetText(state.IsActive ? "POWER" : string.Empty, slotStackLabels[i]);
                if (slotStackLabels[i] != null) slotStackLabels[i].color = state.IsActive ? TextColor : MutedColor;
                if (slotFeedback[i] > 0f) continue;
                slotImages[i].color = state.IsActive || i == circuit.HighlightIndex ? HighlightColor : SlotColor;
            }
        }

        public void SetStatus(string status)
        {
            if (modifierFeedbackRemaining > 0f)
            {
                statusBeforeModifier = status;
                return;
            }

            SetText(status, statusText);
            if (statusText != null) statusText.color = MutedColor;
        }

        public void ShowModifierFeedback(SkillModifierType type)
        {
            if (statusText == null) return;
            if (modifierFeedbackRemaining <= 0f)
            {
                statusBeforeModifier = statusText.text;
                statusBeforeModifierColor = statusText.color;
            }

            statusText.text = $"STATUS: {type.ToString().ToUpperInvariant()}";
            statusText.color = ModifierColor(type);
            modifierFeedbackRemaining = 1f;
        }

        public void SetBottomVisible(bool visible)
        {
            if (bottomPanel != null) bottomPanel.SetActive(visible);
        }

        private bool BindTemplate()
        {
            if (gameplayCanvas == null) gameplayCanvas = GetComponentInChildren<Canvas>();
            if (gameplayCanvas == null)
            {
                Debug.LogError($"{nameof(GameplayHud)} needs its Canvas assigned in the Inspector.");
                return false;
            }

            Transform root = gameplayCanvas.transform;
            safeArea = root.Find("SafeArea") as RectTransform;
            Transform content = safeArea != null ? safeArea : root;
            Transform bottom = content.Find("bottom") ?? content.Find("CircuitPanel");
            bottomPanel = bottom != null ? bottom.gameObject : null;
            Transform energyPanel = content.Find("bottom/energy") ?? content.Find("CircuitPanel/EnergyPanel") ?? bottom;
            waveText = FindDeep(content, "Wave")?.GetComponent<Text>();
            statusText = FindDeep(content, "Status")?.GetComponent<Text>();
            goldText = FindDeep(content, "Gold")?.GetComponent<Text>();
            energyText = FindDeep(content, "Energy")?.GetComponent<Text>();
            rollResultText = FindDeep(content, "RollResult")?.GetComponent<Text>();
            rollPopup = FindDeep(content, "RollPopup")?.gameObject;
            rollPopupText = rollPopup != null ? rollPopup.GetComponentInChildren<Text>() : null;
            modeButton = FindDeep(energyPanel, "ModeButton")?.GetComponent<Button>();
            rollButton = FindDeep(energyPanel, "RollButton")?.GetComponent<Button>();
            modeButtonLabel = modeButton != null ? modeButton.GetComponentInChildren<Text>() : null;

            Transform slots = FindDeep(energyPanel, "Slots");
            for (int i = 0; i < slotImages.Length; i++)
            {
                Transform slot = slots != null ? slots.Find($"Slot{i}") : null;
                if (slot == null) continue;
                slotImages[i] = slot.GetComponent<Image>();
                slotIndexLabels[i] = FindDeep(slot, "Index")?.GetComponent<Text>();
                slotLabels[i] = FindDeep(slot, "Label")?.GetComponent<Text>();
                slotStackLabels[i] = FindDeep(slot, "Stack")?.GetComponent<Text>();
                slotBaseColors[i] = slotImages[i] != null ? slotImages[i].color : SlotColor;
            }

            string[] speedNames = { "Speed1", "Speed2", "Speed4" };
            for (int i = 0; i < speedButtons.Length; i++)
            {
                speedButtons[i] = FindDeep(content, speedNames[i])?.GetComponent<Button>();
            }

            resultOverlay = FindDeep(root, "ResultOverlay")?.gameObject;
            resultTitle = FindDeep(root, "ResultTitle")?.GetComponent<Text>();
            resultRewards = FindDeep(root, "ResultRewards")?.GetComponent<Text>();
            resultContinueButton = FindDeep(root, "Continue")?.GetComponent<Button>();
            resultContinueLabel = resultContinueButton != null ? resultContinueButton.GetComponentInChildren<Text>() : null;
            Button homeButton = FindDeep(root, "Home")?.GetComponent<Button>();

            Wire(modeButton, () => SetManualMode(!manualMode));
            Wire(rollButton, () => rollRequested?.Invoke());
            for (int i = 0; i < speedButtons.Length; i++)
            {
                float speed = i == 0 ? 1f : i == 1 ? 2f : 4f;
                Wire(speedButtons[i], () => SetSpeed(speed));
            }
            Wire(homeButton, () => returnHome?.Invoke());
            return true;
        }

        private static void Wire(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button == null) return;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(action);
        }

        private static Transform FindDeep(Transform root, string name)
        {
            if (root == null) return null;
            for (int i = 0; i < root.childCount; i++)
            {
                Transform child = root.GetChild(i);
                if (child.name == name) return child;
                Transform found = FindDeep(child, name);
                if (found != null) return found;
            }
            return null;
        }

        public void ActivateSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= slotImages.Length || slotImages[slotIndex] == null) return;
            slotImages[slotIndex].color = HighlightColor;
            slotFeedback[slotIndex] = 0.65f;
        }

        public void ShowResult(bool victory, BattleRunRewards rewards)
        {
            if (resultOverlay == null || rewards == null) return;
            resultOverlay.SetActive(true);
            SetText(victory ? "VICTORY" : "DEFEAT", resultTitle);
            if (resultTitle != null) resultTitle.color = victory ? AccentColor : ParseColor("FB7185");
            SetText($"{rewards.DefeatedMonsters} MONSTERS DEFEATED\n+{rewards.Experience} EXP   +{rewards.Gold} GOLD", resultRewards);
            SetText(victory ? "NEXT LEVEL" : "RETRY", resultContinueLabel);
            if (resultContinueButton != null)
            {
                resultContinueButton.onClick.RemoveAllListeners();
                resultContinueButton.onClick.AddListener(() => continueRun?.Invoke(victory));
            }
        }

        private void SetManualMode(bool manual, bool notify = true)
        {
            manualMode = manual;
            SetText(manual ? "MANUAL" : "AUTO", modeButtonLabel);
            if (notify) setManualMode?.Invoke(manual);
        }

        private void SetSpeed(float speed)
        {
            setTimeScale?.Invoke(speed);
            for (int i = 0; i < speedButtons.Length; i++)
            {
                if (speedButtons[i] == null) continue;
                Image image = speedButtons[i].GetComponent<Image>();
                if (image != null) image.color = speedButtons[i].name == $"Speed{speed:0}" ? AccentColor : SlotColor;
            }
        }

        private static void SetText(string value, Text text)
        {
            if (text != null) text.text = value;
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
