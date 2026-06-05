using _KITSystem.Utils;
using UnityEngine;

namespace _FightCode.Battle.Logic
{
    public struct Stat
    {
        public int Attack;
        public int Defence;
        public int MaxHealth;
        public int Health;
        public int Shield;
        public float CriticalRate;
        public float CriticalDamage;
        public float AttackSpeed; // cooldown = 1/AttackSpeed
        public int DamageReduction;
    }

    public static class Formula
    {
        private const float K = 1000;

        public static int Damage(Stat caster, Stat target, float skillDamageScale, int skillDamageFlat)
        {
            float baseDamage = caster.CriticalDamage * skillDamageScale;
            float damage = baseDamage + skillDamageFlat;
            bool critical = caster.CriticalRate >= RandomUtils.Value;
            float calculatedDamage = critical ? damage + baseDamage * caster.CriticalDamage : damage;
            float reduce = K / (K + target.Defence);
            float finalDamage = calculatedDamage - calculatedDamage * reduce;
            return (int)finalDamage - target.DamageReduction;
        }

        public static int Power(Stat source, float skillDamageScale, int skillDamageFlat)
        {
            float critFactor = 1f + source.CriticalRate * source.CriticalDamage;
            float averageHit = (source.Attack * skillDamageScale + skillDamageFlat) * critFactor;
            float dps = averageHit * source.AttackSpeed;
            float damageMultiplier = K / (K + source.Defence);
            float ehp = (source.MaxHealth + source.Shield) / damageMultiplier;
            ehp += source.DamageReduction * 20f;
            float power = Mathf.Sqrt(dps * ehp);
            return Mathf.RoundToInt(power);
        }
    }
}