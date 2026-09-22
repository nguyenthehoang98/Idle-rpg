using System;
using UnityEngine;
using UnityEngine.UI;

namespace _TDS.Gameplay
{
    public class EnergyBar : MonoBehaviour
    {
        [SerializeField] private Image imageFill;
        [SerializeField] private Button button;
        [SerializeField] CircuitTickRunner circuitRunner;
        
        private int previous;

        private void Awake()
        {
            button.onClick.AddListener(() => { });
        }

        private void Update()
        {
            if (circuitRunner.Circuit != null && previous != circuitRunner.Circuit.Energy)
            {
                previous = circuitRunner.Circuit.Energy;
                imageFill.fillAmount = previous / (float)circuitRunner.Circuit.EnergyCapacity;
            }
        }
    }
}