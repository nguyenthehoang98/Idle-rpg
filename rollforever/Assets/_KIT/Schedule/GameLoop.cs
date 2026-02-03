using UnityEngine;

namespace _KIT.Schedule
{
    [DefaultExecutionOrder(-10000)]
    public sealed class GameLoop : MonoBehaviour, ITickSubscribe
    {
        readonly TickSystem tickSystem = new TickSystem();

        [Range(5, 60), SerializeField] private int fps;

        public void Pause() => IsPaused = true;

        public void Resume() => IsPaused = false;

        public void Register(ITick module) => tickSystem.pendingAdd.Enqueue(module);

        public void UnRegister(ITick module) => tickSystem.pendingRemove.Enqueue(module);

        public void SetTimeScale(float value) => ScaleTime = Mathf.Max(1, value);

        public void SetFpsRate(int rate) => FrameDeltaTime = 1f / rate;

        public double Time { get; private set; }

        public float ScaleTime { get; private set; } = 1;

        public float FrameDeltaTime { get; private set; } = 0.05f; // 10 fps

        public bool IsPaused { get; private set; }

        private float elapsed;

        private void Awake()
        {
            FrameDeltaTime = 1f / fps;
        }

        void Update()
        {
            if (IsPaused)
                return;

            float dt = UnityEngine.Time.deltaTime;
            elapsed += dt;
            Time += dt;
            if (elapsed >= FrameDeltaTime)
            {
                tickSystem.Update(FrameDeltaTime * ScaleTime);
                elapsed -= FrameDeltaTime;
            }
        }
    }
}