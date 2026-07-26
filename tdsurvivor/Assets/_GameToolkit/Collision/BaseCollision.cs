using System;
using UnityEngine;
using UnityEngine.Events;

namespace _GameToolkit.Collision
{
   internal abstract class BaseCollision : MonoBehaviour, ICollision
   {
      [SerializeField] protected LayerMask layerMask;

      [SerializeField] private UnityEvent<Shared.EntityId> OnEventOverlap;

      public event Action<Shared.EntityId> OnOverlap;

      public void Startup()
      {
         OnStartUp();
         
         CollisionUpdater.Instance.Add(this);
      }

      public void Tick()
      {
         if (enabled)
         {
            OnTick();
         }
      }

      protected virtual void OnEnable()
      {
      }

      protected virtual void OnStartUp()
      {
      }

      protected abstract void OnTick();

      protected virtual void OnShutdown()
      {
      }

      protected virtual void OnDisable()
      {
         CollisionUpdater.Instance.Remove(this);
      }

      public void Shutdown()
      {
         CollisionUpdater.Instance.Remove(this);
       
         OnShutdown();
      }

      protected void Overlap(Shared.EntityId entityId)
      {
         OnEventOverlap?.Invoke(entityId);
         
         OnOverlap?.Invoke(entityId);
      }
   }
}
