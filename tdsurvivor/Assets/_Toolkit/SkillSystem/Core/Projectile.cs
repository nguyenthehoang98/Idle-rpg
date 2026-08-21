using System;
using System.Collections.Generic;
using _Toolkit.Collider;
using _Toolkit.ResourceManagement;
using _Toolkit.Updater;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Toolkit.SkillSystem.Core
{
    public abstract class Projectile : MonoBehaviour, ITickRunner
    {
        [SerializeField, SerializeReference] protected List<BaseCollisionDetector> detectors;

        protected bool IsActivated { get; private set; }
        protected Vector3 Start { get; private set; }
        protected Vector3 Destination { get; private set; }
        protected Vector3 Direction { get; private set; }
        protected ProjectileContext Context { get; private set; }

        private bool QueueDestroy { get; set; }
        protected bool IsDestroyed { get; private set; }
        protected float FixedElapsedTime { get; private set; }
        protected float EngineDeltaTime { get; private set; }
        
        public event Action OnDestroy;
        public event Action<ProjectilePhase> OnPhaseChanged; 

        public ICollisionDetector[] CollisionDetectors()
        {
            ICollisionDetector[] array = new ICollisionDetector[detectors.Count];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = detectors[i];
            }

            return array;
        }

        protected virtual void Awake()
        {
            EngineDeltaTime = Time.deltaTime;
        }

        /*
         * Vì bắn sẽ tính từ center tới vị trí kẻ địch,
         * nên hàm này sẽ tính toán lại from, to, lifetime với mỗi kiểu đạn
         */
        public abstract void EnsureValid(out float lifetime);

        /*
         * tùy trường hợp sẽ phải tính lại vị trí
         */
        public void Initialize(Vector3 start, Vector3 destination, ProjectileContext context)
        {
            EvaluatePosition(ref start, ref destination);
            Start = start;
            Destination = destination;
            Direction = (destination - start).normalized;
            Context = context;

            transform.localScale = Vector3.one * context.SizeScale;
            
            FixedElapsedTime = 0;
            TransitionDeltaPosition = Vector3.zero;
            TransitionPreviousPosition = Vector3.zero;
            
            OnDestroy = null;
            OnPhaseChanged = null; // tránh cộng dồn handler khi pool tái sử dụng
        }

        public void Startup()
        {
            transform.position = Start;
            
            OnStartup();
            
            ProjectileTickRunner.Instance.Add(this);

            IsActivated = true;
            IsDestroyed = false;
            QueueDestroy = false;
        }

        public void Tick(float deltaTime)
        {
            if (IsActivated)
            {
                FixedElapsedTime = deltaTime;
                EngineDeltaTime = deltaTime;
                OnTick(deltaTime);
            }
        }

        protected virtual void FixedUpdate()
        {
            if (!IsDestroyed && FixedElapsedTime < TransitionDuration)
            {
                FixedElapsedTime += Time.fixedDeltaTime;

                float t = Mathf.Clamp01(FixedElapsedTime / EngineDeltaTime);

                transform.position = Vector3.Lerp(TransitionPreviousPosition, Start + TransitionDeltaPosition, t);

                if (t >= 1 && QueueDestroy) Destroy();
            }
        }

        public void Shutdown()
        {
            QueueDestroy = true;
            IsActivated = false;

            OnShutdown();

            ProjectileTickRunner.Instance.Remove(this);
        }

        protected virtual void OnStartup()
        {
        }

        protected abstract void OnTick(float deltaTime);

        protected virtual void OnShutdown()
        {
        }
        
        protected abstract float TransitionDuration { get; }

        protected abstract Vector3 TransitionDeltaPosition { get; set; }

        protected abstract Vector3 TransitionPreviousPosition { get; set; }

        protected virtual void EvaluatePosition(ref Vector3 start, ref Vector3 destination)
        {
        }

        public virtual void EvaluateTextDamagePosition(ref Vector3 position)
        {
            position = Start + TransitionDeltaPosition;
        }

        protected void ChangePhase(ProjectilePhase phase)
        {
            OnPhaseChanged?.Invoke(phase);
        }
        
        private async void Destroy()
        {
            OnDestroy?.Invoke();
            
            IsDestroyed = true;

            gameObject.SetActive(false);

            await UniTask.NextFrame(PlayerLoopTiming.Update);

            Pool.Destroy(gameObject);
        }
    }
}