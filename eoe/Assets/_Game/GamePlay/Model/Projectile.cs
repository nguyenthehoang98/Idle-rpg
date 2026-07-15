using System;
using _KITSystem.Resource;
using _KITSystem.SkillSystem.Imp;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace _Game.GamePlay.Model
{
    public class Projectile : MonoBehaviour
    {
        public Vector3 TargetPosition { get; private set; }
        public Vector3 PreviousPosition { get; private set; }

        [SerializeField] private bool enableDestroy = true;
        [SerializeField] private Transform rotatePivot;
        [SerializeField] private TrailRenderer trailRenderer;
        [SerializeField] private ColliderData colliderData;

        public ColliderData ColliderData => colliderData;

        private Action onDestroyCallback;
        private float elapsedTime;
        private float deltaTime;
        private bool isRunning = false;
        private bool shouldDestroy = false;

        private void OnDrawGizmos()
        {
#if UNITY_EDITOR
            Vector2 position = transform.position;
            Vector2 center = position + colliderData.relativePosition;
            switch (colliderData.type)
            {
                case ColliderType.Circle:
                    Handles.DrawWireDisc(center, Vector3.forward, colliderData.circleRadius);
                    break;
                default: 
                    Debug.LogError("Error in DrawGizmosSelected");
                    break;
            }
#endif
        }

        public void Initialize()
        {
            elapsedTime = 0;
            TargetPosition = PreviousPosition = transform.position;
            
            shouldDestroy = false;
            isRunning = true;

            EnableTrail();
        }

        public void SetPosition(Vector3 position, float dt)
        {
            this.PreviousPosition = transform.position;
            this.TargetPosition = position;
            this.deltaTime = dt;
            this.elapsedTime = 0;
            
            Vector3 direction = position - PreviousPosition;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            rotatePivot.localRotation = Quaternion.Euler(0, 0, angle + 90f);
        }

        private void FixedUpdate()
        {
            if (!isRunning) return;

            elapsedTime += Time.fixedDeltaTime;

            float t = Mathf.Clamp01(elapsedTime / deltaTime);

            transform.position = Vector3.Lerp(PreviousPosition, TargetPosition, t);

            if (t >= 1.0f && shouldDestroy)
            {
                if (onDestroyCallback != null)
                {
                    onDestroyCallback.Invoke();
                    onDestroyCallback = null;

                    DisableTrail();
                }
                
                if(enableDestroy) Pool.Destroy(gameObject);
                
                isRunning = false;
            }
        }

        public void Destroy(Action callback)
        {
            if (shouldDestroy) return;

            onDestroyCallback = callback;
            
            shouldDestroy = true;
        }

        public void EnableTrail()
        {
            if (trailRenderer != null)
            {
                trailRenderer.emitting = true;
                trailRenderer.enabled = true;
            }
        }

        public void DisableTrail()
        {
            if (trailRenderer != null)
            {
                trailRenderer.emitting = false;
                trailRenderer.enabled = false;
                
                trailRenderer.Clear();
            }
        }
    }
}
