using System;

namespace _KITSystem.Formula
{
    [Serializable]
    public readonly struct StatSnapshot
    {
        private const float DEFENSE_K = 1000f;
        
        public readonly float Attack;
        public readonly float Health;
        public readonly float Defense;
        public readonly float AttackSpeed;
        public readonly float CriticalRate;
        public readonly float CriticalDamage;
        public readonly float DamageMultiplier;
        public readonly float ArmorPenPercent;

        public StatSnapshot(float attack, float health, float defense, float attackSpeed,
            float criticalRate, float criticalDamage, float damageMultiplier, float armorPenPercent)
        {
            Attack = attack;
            Health = health;
            Defense = defense;
            AttackSpeed = attackSpeed;
            CriticalRate = criticalRate;
            CriticalDamage = criticalDamage;
            DamageMultiplier = damageMultiplier;
            ArmorPenPercent = armorPenPercent;
        }

        public float Dps(float skillScaleDamage = 1f, float skillFlatDamage = 0)
        {
            float criticalFactor = 1f + CriticalRate * CriticalDamage;
            float skillDamage = Attack * Math.Max(0, skillScaleDamage) + Math.Max(0, skillFlatDamage);
            return skillDamage * AttackSpeed * criticalFactor * DamageMultiplier;
        }

        public float DamageReduction()
        {
            float effectiveDefense = Math.Max(0, Defense * (1f - ArmorPenPercent));
            return effectiveDefense / (effectiveDefense + DEFENSE_K);
        }

        public float EffectiveHp()
        {
            float damageTakenRate = Math.Max(0.0001f, 1f - DamageReduction());
            return Math.Max(0, Health) / damageTakenRate;
        }
    }
}