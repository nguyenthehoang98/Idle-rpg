using System.Collections.Generic;
using _Game.Configs;
using Unity.Collections;

namespace _Game.Battle.Data
{
    public struct StatData
    {
        NativeHashMap<int, Stat> map;
        NativeHashMap<int, StatModifier> modifiers;

        public StatData Insert(StatType type, Stat stat)
        {
            if (!map.IsCreated) map = new NativeHashMap<int, Stat>(10, Allocator.Persistent);
            map.Add((int)type, stat);
            return this;
        }

        public bool TryGetValue(StatType type, out Stat stat)
        {
            return map.TryGetValue((int)type, out stat);
        }

        public void AddModifier(StatType type, StatModifier modifier)
        {
            if (TryGetValue(type, out Stat stat))
            {
                stat.AddModifier(modifier);
                map[(int)type] = stat;
            }
        }

        public void RemoveModifier(StatType type, StatModifier modifier)
        {
            if (TryGetValue(type, out Stat stat))
            {
                stat.RemoveModifier(modifier);
                map[(int)type] = stat;
            }
        }

        public void ReplaceModifier(List<BuffConfig.BuffData> buffDatas)
        {
            if (modifiers.IsCreated)
            {
                foreach (var m in modifiers)
                {
                    RemoveModifier((StatType)m.Key, m.Value);
                }
            }
            else
            {
                modifiers = new NativeHashMap<int, StatModifier>(buffDatas.Count, Allocator.Persistent);
            }

            modifiers.Clear();

            foreach (var buffData in buffDatas)
            {
                var modifier = new StatModifier(StatModifierType.Additive, buffData.Value);
                modifiers.Add((int)buffData.StatType, modifier);
                AddModifier(buffData.StatType, modifier);
            }
        }
    }
}