using System;
using System.Collections.Generic;

namespace _KITSystem.Formula
{
    [Serializable]
    public sealed class StatComplex
    {
        private readonly Dictionary<StatType, StatValue> stats = new Dictionary<StatType, StatValue>();

        public void SetBase(StatType statType, float value) 
            => GetOrCreate(statType).SetBase(value);

        public void AddBase(StatType statType, float value) 
            => GetOrCreate(statType).AddBase(value);

        public float GetBase(StatType statType)
            => stats.TryGetValue(statType, out StatValue stat) ? stat.BaseValue : 0;

        public float GetValue(StatType statType)
            => stats.TryGetValue(statType, out StatValue stat) ? stat.GetValue() : 0;

        public void AddModifier(StatModifier modifier)
            => GetOrCreate(modifier.StatType).AddModifier(modifier);

        public bool RemoveModifier(StatModifier modifier)
            => stats.TryGetValue(modifier.StatType, out StatValue stat) && stat.RemoveModifier(modifier);

        public int RemoveModifiersFrom(object source)
        {
            if (source == null) return 0;

            int removed = 0;
            foreach (StatValue stat in stats.Values)
            {
                removed += stat.RemoveModifiersFrom(source);
            }

            return removed;
        }
        
        public void ClearModifiers(StatType statType)
        {
            if (stats.TryGetValue(statType, out StatValue stat)) stat.ClearModifiers();
        }

        public void ClearAllModifiers()
        {
            foreach (StatValue stat in stats.Values)
            {
                stat.ClearModifiers();
            }
        }

        public StatSnapshot CreateSnapshot()
        {
            return new StatSnapshot(
                GetValue(StatType.Attack),
                GetValue(StatType.MaxHealth),
                GetValue(StatType.Defense),
                Math.Max(0, GetValue(StatType.AttackSpeed)),
                Clamp01(GetValue(StatType.CriticalRate)),
                Math.Max(0, GetValue(StatType.CriticalDamage)),
                Math.Max(0, 1f + GetValue(StatType.DamageMultiplier)),
                Clamp01(GetValue(StatType.ArmorPenPercent)));
        }

        public float GetPower(float skillScaleDamage = 1f, float skillFlatDamage = 0)
        {
            StatSnapshot snapshot = CreateSnapshot();
            return (float)Math.Sqrt(snapshot.Dps(skillScaleDamage, skillFlatDamage) * snapshot.EffectiveHp());
        }

        private StatValue GetOrCreate(StatType statType)
        {
            if (stats.TryGetValue(statType, out StatValue stat)) return stat;

            stat = new StatValue();
            stats.Add(statType, stat);
            return stat;
        }

        private static float Clamp01(float value)
        {
            if (value < 0) return 0;
            if (value > 1) return 1;
            return value;
        }
    }
}