using System.Collections.Generic;
using UnityEngine;

namespace _KITSystem.Schedule
{
    public class TickSystemOwner : MonoBehaviour
    {
        [SerializeField] private int targetFPS = 30;
        [SerializeField] private DataTemp[] list;
        private TickSystem tickSystem;
        private float tickInterval;
        private float accumulator;
        
        private void Awake()
        {
            Application.runInBackground = true;
            Application.targetFrameRate = 60;
            tickInterval = 1f / targetFPS;
            
            Dictionary<TickGroup, List<ITickable>> ticks = new Dictionary<TickGroup, List<ITickable>>();
            foreach (var dataTemp in list)
            {
                if (ticks.ContainsKey(dataTemp.group))
                {
                    ticks[dataTemp.group].Add(dataTemp.tickable);
                }
                else
                {
                    ticks.Add(dataTemp.group, new List<ITickable> { dataTemp.tickable });
                }
            }

            tickSystem = new TickSystem(ticks);
        }

        void Update()
        {
            accumulator += Time.deltaTime;

            while (accumulator >= tickInterval)
            {
                float dt = tickInterval;

                tickSystem.Run(TickGroup.PreUpdate, dt);
                tickSystem.Run(TickGroup.Update, dt);
                tickSystem.Run(TickGroup.PostUpdate, dt);

                accumulator -= tickInterval;
            }
        }

        public bool TryGetTickable<T>(out T tickable) where T : ITickable
        {
            foreach (var dataTemp in list)
            {
                if (dataTemp.tickable is T t)
                {
                    tickable = t;
                    return true;
                }
            }

            tickable = default;
            return false;
        }

        [System.Serializable]
        class DataTemp
        {
            public TickGroup group;
            [SerializeReference] public ITickable tickable;
        }
    }
}