using System.Collections.Generic;

namespace _KIT.Schedule
{
    sealed class TickSystem
    {
        List<ITick> modules = new List<ITick>();

        internal Queue<ITick> pendingAdd = new Queue<ITick>();
        internal Queue<ITick> pendingRemove = new Queue<ITick>();


        internal void Update(float dt)
        {
            while (pendingRemove.Count > 0)
            {
                var item = pendingRemove.Dequeue();
                modules.Remove(item);
            }
            
            while (pendingAdd.Count > 0)
            {
                var item = pendingAdd.Dequeue();
                modules.Add(item);
                modules.Sort((a, b) => a.Order.CompareTo(b.Order));
            }

            for (var i = 0; i < modules.Count; i++)
            {
                modules[i].OnUpdate(dt);
            }
        }
    }
}