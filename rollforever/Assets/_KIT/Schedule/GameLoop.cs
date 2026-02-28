using UnityEngine;

namespace _KIT.Schedule
{
    [DefaultExecutionOrder(-10000)]
    public sealed class GameLoop : MonoBehaviour, ITickSubscribe
    {
        readonly TickSystem tickSystem = new TickSystem();
        
        [Range(2, 60), SerializeField] private int fps;
        [Range(0.2f, 20.0f), SerializeField] private float speed = 1f;
        
        public void Pause() => IsPaused = true;
        public void Resume() => IsPaused = false;
        public void Register(ITick module) => tickSystem.pendingAdd.Enqueue(module);
        public void UnRegister(ITick module) => tickSystem.pendingRemove.Enqueue(module);
        public void SetFpsRate(int rate) => FrameDeltaTime = 1f / rate;
        public void SetSpeed(float value) => speed = value;

        public double Time { get; private set; }
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

            float dt = UnityEngine.Time.deltaTime * speed;
            elapsed += dt;
            Time += dt;
            while (elapsed >= FrameDeltaTime)
            {
                tickSystem.Update(FrameDeltaTime);
                elapsed -= FrameDeltaTime;                
            }
        }
    }
}