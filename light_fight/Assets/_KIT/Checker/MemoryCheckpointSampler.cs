using System.Collections.Generic;

namespace _KIT.Checker
{
    public class MemoryCheckpointSampler
    {
        Dictionary<string, long> checkpoints = new Dictionary<string, long>();

        /// Gọi tại bất kỳ event nào (load, spawn, unload…)
        public void Mark(string key)
        {
            checkpoints[key] = 0;//Profiler.GetTotalUsedMemoryLong();
        }

        /// So sánh 2 mốc
        public long Delta(string from, string to)
        {
            if (!checkpoints.ContainsKey(from) || !checkpoints.ContainsKey(to))
                return 0;

            return checkpoints[to] - checkpoints[from];
        }

        /// Lấy raw value
        public long Get(string key)
        {
            return checkpoints.TryGetValue(key, out var v) ? v : 0;
        }

        public void Clear()
        {
            checkpoints.Clear();
        }
    }
}