using _TDS.GameConfig;
using UnityEngine;

namespace _TDS.Battle
{
    public static class SkillModifierSelector
    {
        public static bool TrySelect(
            SkillModifierData[] modifiers,
            float weightRoll,
            float successRoll,
            out SkillModifierData selected)
        {
            selected = default;
            if (modifiers == null || modifiers.Length == 0) return false;

            float totalWeight = 0f;
            for (int i = 0; i < modifiers.Length; i++)
            {
                totalWeight += Mathf.Max(0f, modifiers[i].weight);
            }

            if (totalWeight <= 0f) return false;

            float pick = Mathf.Clamp01(weightRoll) * totalWeight;
            for (int i = 0; i < modifiers.Length; i++)
            {
                float weight = Mathf.Max(0f, modifiers[i].weight);
                if (weight <= 0f) continue;
                if (pick > weight)
                {
                    pick -= weight;
                    continue;
                }

                selected = modifiers[i];
                return successRoll < Mathf.Clamp01(selected.successRate);
            }

            selected = modifiers[modifiers.Length - 1];
            return successRoll < Mathf.Clamp01(selected.successRate);
        }
    }
}
