using System.Collections.Generic;
using _Toolkit.Statistics;

namespace _TDS.Statistics
{
    public sealed class Stats
    {
        private Dictionary<int, Stat> stats = new Dictionary<int, Stat>();

        public bool AddStat(int id, Stat stat)
        {
            return stats.TryAdd(id, stat);
        }

        public bool RemoveStat(int id)
        {
            return stats.Remove(id);
        }

        public bool TryGet(int id, out Stat stat)
        {
            return stats.TryGetValue(id, out stat);
        }
    }
}