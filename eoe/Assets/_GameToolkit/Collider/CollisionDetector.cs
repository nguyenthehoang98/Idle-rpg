using System;
using _GameToolkit.Shared;
using _GameToolkit.Updater;
using UnityEngine;

namespace _GameToolkit.Collider
{
    [Serializable]
    public abstract class CollisionDetector : MonoBehaviour, ITickRunner
    {
        [SerializeField] private int maxResults = 10;
        [SerializeField] protected LayerMask layerMask;

        protected Collider2D[] Results;
        protected ContactFilter2D ContactFilter;

        public event Action<Unique> OnOverlapped;

        protected virtual void Awake()
        {
            Results = new Collider2D[maxResults];
            ContactFilter = new ContactFilter2D();
            ContactFilter.useLayerMask = true;
            ContactFilter.layerMask = layerMask;
        }

        protected virtual void OnEnable()
        {
        }

        protected virtual void OnDisable()
        {
            Shutdown();
        }

        protected virtual void OnDestroy()
        {
            Shutdown();
        }

        public void Startup()
        {
            ColliderTickRunner.Instance.Add(this);
            OnStartup();
        }

        public void Shutdown()
        {
            OnShutdown();
            ColliderTickRunner.Instance.Remove(this);
        }

        protected void Overlapped(Unique unique)
        {
            if (unique != null) OnOverlapped?.Invoke(unique);
        }

        protected virtual void OnStartup()
        {
        }

        protected virtual void OnShutdown()
        {
        }

        public abstract void Tick(float deltaTime);
    }
}