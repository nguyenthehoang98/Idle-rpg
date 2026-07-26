using System;
using UnityEngine;
using UnityEngine.Events;
using EntityId = _GameToolkit.Entity.EntityId;

namespace _GameToolkit.Collision
{
   public abstract class BaseCollision : MonoBehaviour
   {
      [SerializeField] protected LayerMask layerMask;

      [SerializeField] private UnityEvent<EntityId> OnEventOverlap;

      public event Action<EntityId> OnOverlap;

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

      protected void Overlap(EntityId entityId)
      {
         OnEventOverlap?.Invoke(entityId);
         
         OnOverlap?.Invoke(entityId);
      }
   }
}