using _TDS.Battle;
using _TDS.Utils;
using TMPro;
using UnityEngine;

namespace _TDS.Gameplay
{
    public class HealthBar : MonoBehaviour
    {
        [SerializeField] private MaterialPropertySetter property;
        [SerializeField] private TextMeshPro[] texts;

        private int previous;
        
        private void Update()
        {
            if (previous != PlayerVitals.CurrentHealth)
            {
                previous = PlayerVitals.CurrentHealth;
                property.SetFloat(1 - previous / (float)PlayerVitals.MaxHealth);

                for (int i = 0; i < texts.Length; i++)
                {
                    texts[i].text = previous.ToString();
                }
            }
        }
    }
}