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
                }
            }
        }

        public void Init(bool isLocked)
        {
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
            textValue.text = RandomUtils.Range(1, 6).ToString();
        }
        
        public void SetValue(float value) => scale = value;

        public RectTransform RectTransform => rectTransform;
    }
}