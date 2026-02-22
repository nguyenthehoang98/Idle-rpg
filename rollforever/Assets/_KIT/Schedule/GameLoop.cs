using System;
using UnityEngine;

namespace _KIT.Schedule
{
    [DefaultExecutionOrder(-10000)]
    public sealed class GameLoop : MonoBehaviour, ITickSubscribe
    {
        public event Action OnChangeScaleTime;
        
        readonly TickSystem tickSystem = new TickSystem();
        
        [Range(2, 60), SerializeField] private int fps;
        [Range(0.2f, 20.0f), SerializeField] private float speed = 1f;
        [Range(1, 5.0f), SerializeField] private float scaleTime = 1f;
        
        public void Pause() => IsPaused = true;
        public void Resume() => IsPaused = false;
        public void Register(ITick module) => tickSystem.pendingAdd.Enqueue(module);
        public void UnRegister(ITick module) => tickSystem.pendingRemove.Enqueue(module);
        public void SetTimeScale(float value)
        {
            ScaleTime = Mathf.Max(1, value);
            OnChangeScaleTime?.Invoke();
        }
        public void SetFpsRate(int rate) => FrameDeltaTime = 1f / rate;

        public double Time { get; private set; }
        public float ScaleTime { get; private set; } = 1;
        public float FrameDeltaTime { get; private set; } = 0.05f; // 10 fps
        public bool IsPaused { get; private set; }

        private float elapsed;

        private void Awake()
        {
            FrameDeltaTime = 1f / fps;
            SetTimeScale(scaleTime);
        }

        void Update()
        {
            if (IsPaused)
                return;

            float dt = UnityEngine.Time.deltaTime * speed;
            elapsed += dt;
            Time += dt;
            while (elapsed >= FrameDeltaTime)
            {
                tickSystem.Update(FrameDeltaTime * ScaleTime);
                elapsed -= FrameDeltaTime;                
            }
        }
    }
}