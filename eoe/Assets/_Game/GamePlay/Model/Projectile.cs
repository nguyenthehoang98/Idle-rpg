using System;
using _KITSystem.Resource;
using _KITSystem.SkillSystem.Imp;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace _Game.GamePlay.Model
{
    public class Projectile : MonoBehaviour
    {
        [Header("Event & Parameters")]
        [SerializeField] private UnityEvent onInitialize;
        [SerializeField] private UnityEvent onDestroy;
        [SerializeField] private bool canDestroy = true;
        [SerializeField] private bool dependencyRelativePosition = true;
        [SerializeField] private bool shouldPushToWeapon;
        [Header("Components")]
        [SerializeField] private Transform positionPivot;
        [SerializeField] private Transform scalePivot;
        [SerializeField] private Transform rotatePivot;
        [SerializeField] private TrailRenderer trailRenderer;
        [SerializeField] private ColliderData[] colliders = new ColliderData[0];

        public ColliderData[] Colliders { get; private set; }

        private Action onDestroyCallback;

        private Vector3 targetPosition;

        private Vector3 previousPosition;

        private float elapsedTime;

        private float deltaTime;

        private float angle;

        private bool isRunning = false;

        private bool shouldDestroy = false;

        private bool stopped = false;

        private bool blockRotation = false;

        private void OnDrawGizmos()
        {
#if UNITY_EDITOR
            if (Application.isPlaying) return;

            foreach (var colliderData in colliders)
            {
                Vector2 center = (Vector2)transform.position + colliderData.relativePosition;

                Color color = Color.green;

                switch (colliderData.type)
                {
                    case ColliderType.Circle:
                        Handles.color = color;
                        Handles.DrawWireDisc(center, Vector3.forward, colliderData.circleRadius, 2f);
                        Handles.color = new Color(color.r, color.g, color.b, 0.1f);
                        Handles.DrawSolidDisc(center, Vector3.forward, colliderData.circleRadius);
                        break;
                    case ColliderType.Rectangle:
                        Matrix4x4 oldMatrix = Handles.matrix;
                        Handles.matrix = Matrix4x4.TRS(center, transform.rotation, Vector3.one);
                        Vector2 size = colliderData.rectangleSize;
                        Vector3[] verts =
                        {
                            new(-size.x * 0.5f, -size.y * 0.5f, 0),
                            new(-size.x * 0.5f, size.y * 0.5f, 0),
                            new(size.x * 0.5f, size.y * 0.5f, 0),
                            new(size.x * 0.5f, -size.y * 0.5f, 0),
                        };

                        Handles.DrawSolidRectangleWithOutline(verts, new Color(color.r, color.g, color.b, 0.1f), color);
                        Handles.matrix = oldMatrix;
                        break;
                    default:
                        Debug.LogError("Error in DrawGizmosSelected");
                        break;
                }
            }
#endif
        }

        public void Initialize(BaseWeapon weapon)
        {
            blockRotation = false;
            elapsedTime = 0;
            targetPosition = previousPosition = positionPivot.position;

            shouldDestroy = false;
            isRunning = true;

            if (weapon != null && shouldPushToWeapon)
            {
                transform.SetParent(weapon.GetMuzzleTransform());
                transform.localRotation = Quaternion.Euler(0,0,0);
                transform.localPosition = Vector3.zero;
            }

            EnableTrail();

            onInitialize?.Invoke();
        }

        public void ImmediatelySetPosition(Vector3 position)
        {
            positionPivot.position = position;
        }

        public void SetPosition(Vector3 position, Vector3 direction, float dt)
        {
            if (stopped)
            {
                stopped = false;
                return;
            }

            this.previousPosition = positionPivot.position;
            this.targetPosition = position;
            this.deltaTime = dt;
            this.elapsedTime = 0;

            Rotate(direction);
        }

        public void BlockRotation() => blockRotation = true;

        public void Rotate(Vector3 direction)
        {
            if (blockRotation) return;

            angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            //rotatePivot.localEulerAngles = new Vector3(0, 0, angle);
        }

        public void StopLerpMotion()
        {
            stopped = true;

            previousPosition = positionPivot.position;

            targetPosition = positionPivot.position;
        }

        private void FixedUpdate()
        {
            if (!isRunning) return;

            elapsedTime += Time.fixedDeltaTime;

            float t = Mathf.Clamp01(elapsedTime / deltaTime);

            positionPivot.position = Vector3.Lerp(previousPosition, targetPosition, t);

            if (t >= 1.0f && shouldDestroy)
            {
                if (onDestroyCallback != null)
                {
                    onDestroy?.Invoke();
                    
                    onDestroyCallback.Invoke();
                    onDestroyCallback = null;

                    DisableTrail();
                }

                if (canDestroy)
                {
                    if (shouldPushToWeapon) transform.SetParent(null);
                    
                    Pool.Destroy(gameObject);
                }

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
            Colliders = new ColliderData[colliders.Length];

            for (int i = 0; i < Colliders.Length; i++)
            {
                ColliderData colliderData = colliders[i];
                
                colliderData.circleRadius *= scale;
                
                colliderData.rectangleSize *= scale;
                
                if (dependencyRelativePosition) colliderData.relativePosition *= scale;

                Colliders[i] = colliderData;
            }

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

        public void Active()
        {
            gameObject.SetActive(true);
        }

        public void Inactive()
        {
            gameObject.SetActive(false);
        }
    }
}