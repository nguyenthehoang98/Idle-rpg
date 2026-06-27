using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using _KITSystem.Schedule;

namespace _Game.GamePlay.View
{
    [Serializable]
    public class UITickable : ITickable
    {
        private List<IViewTick> ticks = new List<IViewTick>();
        private int count;
        
        public void AddTick(IViewTick tick)
        {
            ticks.Add(tick);
            count++;
        }
        
        public async Task Initialize()
        {
            for (int i = 0; i < count; i++)
            {
                await ticks[i].Initialize();
            }
        }

        public void Tick(float deltaTime)
        {
            for (int i = 0; i < count; i++)
            {
                ticks[i].Tick(deltaTime);
            }
        }

        public void Dispose()
        {
        }
    }
}