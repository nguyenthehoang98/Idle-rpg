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
        [SerializeField] private TextMeshProUGUI textValue;
        [SerializeField] private float cooldown = 1;
        [SerializeField] private Image imgCooldown;
        [SerializeField] private GameObject lockObject;

        private float scale = 0;
        private float elapsedTime;
        private Action<int, int> onTrigger;
        private int order;
        private int value;
        private bool isLocked;
        
        private void Update()
        {
            if (!isLocked)
            {
                float deltaTime = Time.deltaTime;
                float d = deltaTime * (1 + scale);
                elapsedTime += d;
                float f = Mathf.Clamp01(elapsedTime / cooldown);
                imgCooldown.fillAmount = f;

                if (f >= 1)
                {
                    Random();
                    elapsedTime = 0;
                    onTrigger?.Invoke(order, value);
                }
            }
        }

        public void Init(int order, bool isLocked, Action<int, int> onTrigger)
        {
            this.order = order;
            this.onTrigger = onTrigger;
            this.isLocked = isLocked;
            lockObject.SetActive(isLocked);
            textValue.gameObject.SetActive(!isLocked);
            imgCooldown.gameObject.SetActive(!isLocked);
        }

        public void SetColor(Color color)
        {
            imgCooldown.color = color;
        }

        private void Random()
        {
            value = RandomUtils.Range(1, 6);
            textValue.text = value.ToString();
        }
        
        public void SetValue(float value) => scale = value;

        public RectTransform RectTransform => rectTransform;
    }
}