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
        protected Vector3 Goal { get; private set; }
        protected Vector3 Direction { get; private set; }
        protected ProjectileRuntimeData RuntimeData { get; private set; }

        private bool QueueDestroy { get; set; }
        protected bool IsDestroyed { get; private set; }
        protected float FixedElapsedTime { get; private set; }
        protected float EngineDeltaTime { get; private set; }

        protected virtual void Awake()
        {
            EngineDeltaTime = Time.deltaTime;
        }

        public void Startup(Vector3 start, Vector3 goal, ProjectileRuntimeData runtimeData)
        {
            Start = start;
            Goal = goal;
            Direction = (goal - start).normalized;
            RuntimeData = runtimeData;

            FixedElapsedTime = 0;
            TransitionDeltaPosition = Vector3.zero;
            TransitionPreviousPosition = Vector3.zero;
            
            transform.position = start;
            transform.localScale = Vector3.one * runtimeData.SizeScale;
            
            ProjectileTickRunner.Instance.Add(this);

            OnStartup();

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

        private async void Destroy()
        {
            IsDestroyed = true;

            gameObject.SetActive(false);

            await UniTask.NextFrame(PlayerLoopTiming.Update);

            Pool.Destroy(gameObject);
        }
    }
}