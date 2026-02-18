using Unity.Collections;

namespace _Game.Battle.Data
{
    public struct StatData
    {
        NativeHashMap<int, Stat> map;

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
    }
}