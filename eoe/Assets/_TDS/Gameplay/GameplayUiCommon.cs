using _TDS.Battle;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace _TDS.Gameplay
{
    public sealed class GameplayUiCommon : MonoBehaviour
    {
        [Header("Resource UI")]
        [SerializeField] private GameObject health;
        [SerializeField] private GameObject energy;

        private ResourceBar healthBar;
        private ResourceBar energyBar;

        private void Awake()
        {
            healthBar = ResourceBar.Bind(health);
            energyBar = ResourceBar.Bind(energy);
        }

        public void SetHealth(int current, int max)
        {
            healthBar?.Set(current, max);
        }

        public void SetEnergy(EnergyCircuit circuit)
        {
            if (circuit == null) return;
            energyBar?.Set(circuit.Energy, circuit.EnergyCapacity);
        }

        private sealed class ResourceBar
        {
            private readonly Image fill;
            private readonly TMP_Text tmpText;
            private readonly Text legacyText;

            private ResourceBar(Image fill, TMP_Text tmpText, Text legacyText)
            {
                this.fill = fill;
                this.tmpText = tmpText;
                this.legacyText = legacyText;
            }

            public static ResourceBar Bind(GameObject root)
            {
                if (root == null) return null;
                return new ResourceBar(
                    Find<Image>(root.transform, "fill"),
                    Find<TMP_Text>(root.transform, "text"),
                    Find<Text>(root.transform, "text"));
            }

            public void Set(float current, float max)
            {
                float safeMax = Mathf.Max(0f, max);
                float safeCurrent = Mathf.Clamp(current, 0f, safeMax);
                if (fill != null) fill.fillAmount = safeMax > 0f ? safeCurrent / safeMax : 0f;
                string value = $"{Mathf.FloorToInt(safeCurrent)}/{Mathf.FloorToInt(safeMax)}";
                if (tmpText != null) tmpText.text = value;
                else if (legacyText != null) legacyText.text = value;
            }

            private static T Find<T>(Transform root, string objectName) where T : Component
            {
                Transform child = root.Find(objectName);
                return child != null ? child.GetComponent<T>() : null;
            }
        }
    }
}
