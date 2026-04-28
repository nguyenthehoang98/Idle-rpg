using System.Collections.Generic;

namespace _KITSystem.Schedule
{
    internal class TickSystem
    {
        private Dictionary<TickGroup, List<ITickable>> ticks;

        public TickSystem(Dictionary<TickGroup, List<ITickable>> ticks)
        {
            this.ticks = ticks;
        }

        public void Run(TickGroup group, float dt)
        {
            if (ticks.TryGetValue(group, out List<ITickable> tickables))
            {
                foreach (var tickable in tickables)
                {
                    tickable.Tick(dt);
                }
            }
        }
    }
}