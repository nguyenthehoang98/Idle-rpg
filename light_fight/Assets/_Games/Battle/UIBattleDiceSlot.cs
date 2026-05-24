using System;
using _KITSystem.Utils;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Games.Battle
{
    public class UIBattleDiceSlot : MonoBehaviour
    {
        [SerializeField] private DiceRollController rig;
        [TitleGroup("Element")]
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private float cooldown = 1;
        [SerializeField] private Image imgCooldown;
        [SerializeField] private GameObject lockObject;
        [SerializeField] private GameObject unlockObject;
        [SerializeField] private TextMeshProUGUI numberText;

        private float scaleTime;
        private float elapsedTime;
        private Action<int> onTriggerDiceNumber;
        private int value;
        private bool isLocked;
        private bool isInitialized;
        private bool isPaused;

        private void Update()
        {
            if (!isInitialized) return;
            if (isPaused) return;
            if (!isLocked)
            {
                float deltaTime = Time.deltaTime;
                float d = deltaTime * (1 + scaleTime);
                elapsedTime += d;
                SetCooldownProgress(Mathf.Clamp01(elapsedTime / cooldown));

                if (elapsedTime >= cooldown)
                {
                    isPaused = true;
                    elapsedTime = 0;
                    Random();
                }
            }
        }

        public void Init(bool locked, Action<int> onTrigger)
        {
            SetLocked(locked);
            elapsedTime = cooldown;
            onTriggerDiceNumber = onTrigger;
            ShowValue(0);
        }

        public void Play()
        {
            isInitialized = true;
        }

        public void ShowValue(int value)
        {
            numberText.text = value > 0 ? value.ToString() : "?";
        }

        public void SetLocked(bool locked)
        {
            isLocked = locked;
            lockObject.SetActive(locked);
            unlockObject.SetActive(!locked);
            imgCooldown.gameObject.SetActive(!locked);
        }

        public void SetCooldownProgress(float progress)
        {
            imgCooldown.fillAmount = progress;
        }

        public void PlayRoll(int value, Action onComplete)
        {
            rig.Roll(value, onComplete);
        }

        public void SetColor(Color color)
        {
            imgCooldown.color = color;
        }

        private void Random()
        {
            value = RandomUtils.Range(1, 7);
            PlayRoll(value, () =>
            {
                ShowValue(value);
                onTriggerDiceNumber?.Invoke(value);
                isPaused = false;
            });
        }

        public void SetScaleTime(float f) => scaleTime = f;

        public RectTransform RectTransform => rectTransform;
    }
}