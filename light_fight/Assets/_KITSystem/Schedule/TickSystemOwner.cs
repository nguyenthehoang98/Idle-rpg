using System.Collections.Generic;
using UnityEngine;

namespace _KITSystem.Schedule
{
    public class TickSystemOwner : MonoBehaviour
    {
        [SerializeField] private DataTemp[] list;
        private TickSystem tickSystem;

        private void Awake()
        {
            Application.runInBackground = true;
            Application.targetFrameRate = 60;
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
            float deltaTime = Time.deltaTime;
            tickSystem.Run(TickGroup.PreUpdate, deltaTime);
            tickSystem.Run(TickGroup.Update, deltaTime);
            tickSystem.Run(TickGroup.PostUpdate, deltaTime);
        }

        void LateUpdate()
        {
            tickSystem.Run(TickGroup.LateUpdate, Time.deltaTime);
        }

        void FixedUpdate()
        {
            tickSystem.Run(TickGroup.FixedUpdate, Time.fixedDeltaTime);
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