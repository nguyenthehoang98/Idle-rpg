using System;
using System.Net.Mime;
using _KITSystem.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Games.Battle
{
    public class UIBattleDiceSlot : MonoBehaviour
    {
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private float cooldown = 1;
        [SerializeField] private Image imgCooldown;
        [SerializeField] private GameObject lockObject;
        [SerializeField] private GameObject unlockObject;
        [SerializeField] private TextMeshProUGUI numberText;
        [SerializeField] private DiceRollController dice;

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
                float f = Mathf.Clamp01(elapsedTime / cooldown);
                imgCooldown.fillAmount = f;

                if (f >= 1)
                {
                    isPaused = true;
                    Random();
                    elapsedTime = 0;
                }
            }
        }

        public void Init(bool locked, Action<int> onTrigger)
        {
            numberText.text = "?";
            isLocked = locked;
            elapsedTime = float.MaxValue;
            onTriggerDiceNumber = onTrigger;
            lockObject.SetActive(locked);
            unlockObject.SetActive(!locked);
            imgCooldown.gameObject.SetActive(!locked);
        }

        public void Play()
        {
            isInitialized = true;
        }

        public void SetColor(Color color)
        {
            imgCooldown.color = color;
        }

        private void Random()
        {
            value = RandomUtils.Range(1, 6);
            dice.Roll(value, () =>
            {
                numberText.text = value.ToString();
                onTriggerDiceNumber?.Invoke(value);
                isPaused = false;
            });
        }
        
        public void SetScaleTime(float f) => scaleTime = f;

        public RectTransform RectTransform => rectTransform;
    }
}