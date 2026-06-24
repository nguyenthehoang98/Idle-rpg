using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace _KITSystem.Schedule
{
    public class TickSystemOwner : MonoBehaviour
    {
        [SerializeField] private bool useUnscaledTime = false;
        [SerializeField] private int targetFPS = 30;
        [SerializeField, Range(1, 50)] protected float loop = 1;
        [SerializeField, Range(1, 20)] private int maxTicksPerFrame = 5;
        [SerializeReference] public List<ITickable> tickables = new List<ITickable>();

        private float accumulator;
        private int tickableCount;

        public event Action<float> OnChangeScaleTime;

        public bool IsPaused { private get; set; } = true;
        public float TickInterval { get; private set; }

        public float Loop
        {
            get => loop;
            set
            {
                loop = value;
                OnChangeScaleTime?.Invoke(value);
            }
        }

        public async Task Initialize()
        {
            for (int i = 0; i < tickables.Count; i++)
            {
                await tickables[i].Initialize();
            }

            await Task.CompletedTask;
        }

        private void Awake()
        {
            Application.runInBackground = true;
            Application.targetFrameRate = 60;
            TickInterval = 1f / targetFPS;
            tickableCount = tickables.Count;
        }

        private void Update()
        {
            if (IsPaused) return;

            float deltaTime = useUnscaledTime
                ? Time.unscaledDeltaTime
                : Time.deltaTime;

            accumulator += deltaTime * loop;

            int tickExecuted = 0;

            while (accumulator >= TickInterval)
            {
                for (int i = 0; i < tickableCount; i++)
                {
                    tickables[i].Tick(TickInterval);
                }

                accumulator -= TickInterval;

                tickExecuted++;

                // chống spiral of death
                if (tickExecuted >= maxTicksPerFrame * loop)
                {
                    accumulator = 0f;

                    break;
                }
            }
        }

        private void OnDisable()
        {
            foreach (var tickable in tickables)
            {
                tickable.Dispose();
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                accumulator = 0f;
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                accumulator = 0f;
            }
        }

        public bool TryGetTickable<T>(out T tickable) where T : ITickable
        {
            for (int i = 0; i < tickables.Count; i++)
            {
                if (tickables[i] is T tt)
                {
                    tickable = tt;
                    return true;
                }
            }

            tickable = default;
            return false;
        }
    }
}