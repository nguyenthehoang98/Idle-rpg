using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _KITSystem.Schedule
{
    public class TickSystemOwner : MonoBehaviour
    {
        [TitleGroup("Tick Group")] 
        [SerializeField] private bool isPausedDefault = true;
        [SerializeField] private bool useUnscaledTime = false;
        [SerializeField] private int targetFPS = 30;
        [SerializeField, Range(1, 50)] protected float loop = 1;
        [SerializeField, Range(1, 20)] private int maxTicksPerFrame = 5;
        [SerializeReference, HideLabel] public ITickable[] tickables;

        private float tickInterval;
        private float accumulator;
        private bool isPaused;
        private int tickableCount;

        public bool IsPaused
        {
            get => isPaused;
            set => isPaused = value;
        }

        private void Awake()
        {
            isPaused = isPausedDefault;
            Application.runInBackground = true;
            Application.targetFrameRate = 60;
            tickInterval = 1f / targetFPS;
            tickableCount = tickables.Length;
        }

        void Update()
        {
            if (isPaused) return;

            float deltaTime = useUnscaledTime
                ? Time.unscaledDeltaTime
                : Time.deltaTime;

#if UNITY_EDITOR
            accumulator += deltaTime * loop;
#else
            accumulator += deltaTime;
#endif
            
            int tickExecuted = 0;

            while (accumulator >= tickInterval)
            {
                for (int i = 0; i < tickableCount; i++)
                {
                    tickables[i].Tick(tickInterval);
                }

                accumulator -= tickInterval;

                tickExecuted++;

                // chống spiral of death
                if (tickExecuted >= maxTicksPerFrame)
                {
                    accumulator = 0f;
                    break;
                }
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
            for (int i = 0; i < tickableCount; i++)
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