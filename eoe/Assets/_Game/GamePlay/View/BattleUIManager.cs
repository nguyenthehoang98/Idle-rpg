using System.Collections.Generic;
using _Game.Configs;
using _KITSystem.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace _Game.GamePlay.View
{
    public class BattleUIManager : MonoBehaviour
    {
        private const int MAX = 4;
        [FormerlySerializedAs("triggerCollection")] [SerializeField] private ElementTriggerScroll triggerScroll;
        [SerializeField] private Button btnPush;
        [SerializeField] private Element[] elements = new Element[4];
        [SerializeField] private Image imgHealthFill;
        [SerializeField] private TextMeshProUGUI txtHealth;
        [SerializeField] private Image imgEnergyFill;
        [SerializeField] private float energySpeed = 1;

        private ColorSetting colorSetting;
        private readonly List<int> collections = new List<int>();
        private bool isInitialized = false;
        private float energy;

        private void Awake()
        {
            btnPush.onClick.AddListener(PushOut);
        }

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
                PushStack();
            }
        }

        private void PushStack()
        {
            if (collections.Count == MAX)
            {
                foreach (var element in elements)
                {
                    element.Inactive();
                }

                for (var i = 0; i < collections.Count; i++)
                {
                    colorSetting.TryGetColor(collections[i], out ColorData colorData);
                    
                    triggerScroll.Push(colorData.activeColor);
                }

                collections.Clear();
            }

            collections.Add(RandomUtils.Range(0, MAX));

            for (var i = 0; i < collections.Count; i++)
            {
                colorSetting.TryGetColor(collections[i], out ColorData colorData);
                
                elements[i].Active(colorData.activeColor);
            }
        }
        
        private void PushOut()
        {
            if (collections.Count > 0) collections.RemoveAt(collections.Count - 1);
            
            for (var i = 0; i < collections.Count; i++)
            {
                colorSetting.TryGetColor(collections[i], out ColorData colorData);
                elements[i].Active(colorData.activeColor);
            }

            for (int i = collections.Count; i < MAX; i++)
            {
                elements[i].Inactive();
            }
        }
    }
}
