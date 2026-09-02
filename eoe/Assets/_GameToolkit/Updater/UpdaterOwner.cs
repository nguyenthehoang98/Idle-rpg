using System;
using System.Collections.Generic;
using UnityEngine;

namespace _GameToolkit.Updater
{
    public class UpdaterOwner : MonoBehaviour
    {
        [SerializeField] private bool playOnStart = true;
        
        [SerializeField] private bool useUnscaledTime = false;
       
        [SerializeField] private int targetFPS = 30;
       
        [SerializeField, Range(1, 5)] protected float loop = 1;
      
        [SerializeField, Range(1, 10)] private int maxTicksPerFrame = 5;
       
        [SerializeField] private List<BaseUpdatable> updatables = new List<BaseUpdatable>();
        
        private bool isPaused = true;
        
        private float accumulator;
        
        private int tickableCount;

        public event Action<float> OnChangeScaleTime;
        
        public event Action<bool> OnChangePause;

        public bool IsPaused
        {
            private get => isPaused;
            set
            {
                isPaused = value;
                
                OnChangePause?.Invoke(isPaused);
            }
        }

        public float TickInterval { get; private set; }
        
        public static float Time { get; private set; }

        public float Loop
        {
            get => loop;
            set
            {
                loop = value;
                OnChangeScaleTime?.Invoke(value);
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Application.isPlaying) OnChangeScaleTime?.Invoke(loop);
        }
#endif

        private void Awake()
        {
            Application.runInBackground = true;
            Application.targetFrameRate = 60;
            TickInterval = 1f / targetFPS;
            tickableCount = updatables.Count;
            Time = 0;
        }

        private void Start()
        {
            if (playOnStart) IsPaused = false;
        }

        private void Update()
        {
            if (IsPaused) return;

            float deltaTime = useUnscaledTime
                ? UnityEngine.Time.unscaledDeltaTime
                : UnityEngine.Time.deltaTime;

            accumulator += deltaTime * loop;

            int tickExecuted = 0;

            while (accumulator >= TickInterval)
            {
                for (int i = 0; i < tickableCount; i++)
                {
                    updatables[i].Tick(TickInterval);
                }

                accumulator -= TickInterval;
                Time += TickInterval;

                tickExecuted++;

                // chống spiral of death
                if (tickExecuted >= maxTicksPerFrame * loop)
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

        public bool TryGet<T>(out T tickable) where T : BaseUpdatable
        {
            for (int i = 0; i < updatables.Count; i++)
            {
                if (updatables[i] is T tt)
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