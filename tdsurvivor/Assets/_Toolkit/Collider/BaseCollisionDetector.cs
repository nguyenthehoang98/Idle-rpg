using System;
using _Toolkit.Shared;
using UnityEngine;

namespace _Toolkit.Collider
{
    [Serializable]
    public abstract class BaseCollisionDetector : MonoBehaviour, ICollisionDetector
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

        public abstract void Tick(float deltaTime);

        protected virtual void OnShutdown()
        {
        }
    }
}