using System.Collections.Generic;
using _Game.Configs;
using _KITSystem.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.GamePlay.View
{
    public class BattleUIManager : MonoBehaviour
    {
        private const int MAX = 4;
        [SerializeField] private Element[] elements = new Element[4];
        [SerializeField] private Image imgHealthFill;
        [SerializeField] private TextMeshProUGUI txtHealth;
        [SerializeField] private Image imgEnergyFill;
        [SerializeField] private float energySpeed = 1;

        private ColorSetting colorSetting;
        private Queue<int> elementQueue = new Queue<int>();
        private bool isInitialized = false;
        private float energy;

        public void Initialize()
        {
            isInitialized = true;
            colorSetting = ColorSetting.Instance;
        }

        private void Update()
        {
            if (!isInitialized) return;
            
            energy += Time.deltaTime * energySpeed;
            imgEnergyFill.fillAmount = energy;
            if (energy >= 1)
            {
                // todo: play animation energy
                energy = 0;
                imgEnergyFill.fillAmount = 0;
                PushElement();
            }
        }

        private void PushElement()
        {
            if (elementQueue.Count == MAX)
            {
                foreach (var element in elements)
                {
                    element.Inactive();
                }
                elementQueue.Clear();
            }

            elementQueue.Enqueue(RandomUtils.Range(0, MAX));

            int index = 0;
            foreach (var value in elementQueue)
            {
                colorSetting.TryGetColor(value, out ColorData colorData);
                elements[index].Active(colorData.activeColor);
                index++;
            }
        }
    }
}
