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
        [SerializeField] private bool canDestroy = true;
        [SerializeField] private bool dependencyRelativePosition = true;
        [SerializeField] private Transform scalePivot;
        [SerializeField] private Transform rotatePivot;
        [SerializeField] private TrailRenderer trailRenderer;
        [SerializeField] private ColliderData colliderData;

        public ColliderData ColliderData { get; private set; }

        private Action onDestroyCallback;

        private Vector3 targetPosition;
        
        private Vector3 previousPosition;
        
        private float elapsedTime;
        
        private float deltaTime;

        private float angle;
        
        private bool isRunning = false;
        
        private bool shouldDestroy = false;

        private bool stopped = false;

        private void OnDrawGizmos()
        {
#if UNITY_EDITOR
            if (Application.isPlaying) return;
            
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
            targetPosition = previousPosition = transform.position;
            
            shouldDestroy = false;
            isRunning = true;

            EnableTrail();
        }

        public void SetPosition(Vector3 position, float dt)
        {
            if (stopped)
            {
                stopped = false;
                return;
            }
            
            this.previousPosition = transform.position;
            this.targetPosition = position;
            this.deltaTime = dt;
            this.elapsedTime = 0;
            
            Vector3 direction = position - previousPosition;
            angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            rotatePivot.localEulerAngles = new Vector3(0, 0, angle);
        }

        public void StopLerpMotion()
        {
            stopped = true;
            previousPosition = transform.position;
            targetPosition = transform.position;
        }

        private void FixedUpdate()
        {
            if (!isRunning) return;

            elapsedTime += Time.fixedDeltaTime;

            float t = Mathf.Clamp01(elapsedTime / deltaTime);

            transform.position = Vector3.Lerp(previousPosition, targetPosition, t);

            if (t >= 1.0f && shouldDestroy)
            {
                if (onDestroyCallback != null)
                {
                    onDestroyCallback.Invoke();
                    onDestroyCallback = null;

                    DisableTrail();
                }

                if (canDestroy) Pool.Destroy(gameObject);
                
                isRunning = false;
            }
        }

        public void Destroy(Action callback)
        {
            if (shouldDestroy) return;

            onDestroyCallback = callback;
            
            shouldDestroy = true;
        }

        public void SetSizeScale(float scale)
        {
            ColliderData = new ColliderData
            {
                type = colliderData.type,
                
                circleRadius = colliderData.circleRadius * scale,
                
                relativePosition = dependencyRelativePosition ? colliderData.relativePosition * scale : colliderData.relativePosition,
            };
            
            scalePivot.transform.localScale = Vector3.one * scale;

            if (trailRenderer != null)
            {
                trailRenderer.widthMultiplier = scale;
            }
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
