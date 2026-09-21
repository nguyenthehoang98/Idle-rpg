using _TDS.Battle;
using UnityEngine;

namespace _TDS.Gameplay
{
    public sealed class GameplayUiControl : MonoBehaviour
    {
        [SerializeField] private GameplayUiCommon uiCommon;
        [SerializeField] private CircuitTickRunner circuitRunner;

        private void Update()
        {
            if (uiCommon == null) return;

            uiCommon.SetHealth(PlayerVitals.CurrentHealth, PlayerVitals.MaxHealth);
            uiCommon.SetEnergy(circuitRunner?.Circuit);
        }
    }
}
