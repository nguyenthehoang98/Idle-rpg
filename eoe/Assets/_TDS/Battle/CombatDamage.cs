using UnityEngine;

namespace _TDS.Battle
{
    public static class CombatDamage
    {
        public readonly struct DamageResult
        {
            public readonly int Amount;
            public readonly bool IsCritical;

            public DamageResult(int amount, bool isCritical)
            {
                Amount = amount;
                IsCritical = isCritical;
            }
        }

        public static DamageResult Calculate(
            float baseDamage,
            float attack,
            float critChance,
            float critDamage,
            float randomValue)
        {
            bool isCritical = randomValue < Mathf.Clamp01(critChance);
            float multiplier = isCritical ? 1f + Mathf.Max(0f, critDamage) : 1f;
            int amount = Mathf.Max(0, Mathf.RoundToInt(baseDamage * attack * multiplier));
            return new DamageResult(amount, isCritical);
        }

        public static int CalculateLifeSteal(int damage, float lifesteal)
        {
            if (damage <= 0) return 0;
            return Mathf.Clamp(Mathf.RoundToInt(damage * Mathf.Clamp01(lifesteal)), 0, damage);
        }
    }
}
