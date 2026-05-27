using Sirenix.OdinInspector;
using UnityEngine;

namespace _KITSystem.Schedule
{
    public class TickSystemOwner : MonoBehaviour
    {
        [TitleGroup("Tick Group")] 
        [SerializeField] private bool isPausedDefault = true;
        [SerializeField, Range(1, 50)] protected float loop = 1;
        [SerializeField] private int targetFPS = 30;
        [SerializeReference, HideLabel] public ITickable[] tickables;

        private float tickInterval;
        private float accumulator;
        private bool isPaused;

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
        }

        void Update()
        {
            if (isPaused) return;

#if UNITY_EDITOR
            accumulator += Time.deltaTime * loop;
#else
            accumulator += Time.deltaTime;
#endif
            float f = tickInterval * Time.timeScale;
            while (accumulator >= f)
            {
                for (int i = 0; i < tickables.Length; i++)
                {
                    tickables[i].Tick(f);
                }

                accumulator -= f;
            }
        }

        public bool TryGetTickable<T>(out T tickable) where T : ITickable
        {
            foreach (var t in tickables)
            {
                if (t is T tt)
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