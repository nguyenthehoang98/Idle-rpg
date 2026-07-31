using System;
using _GameToolkit.Resource;
using _GameToolkit.SkillSystem.Core;
using _GameToolkit.SkillSystem.Imp;
using _TDS.Skill;
using UnityEngine;

namespace _TDS.Unit
{
    /// <summary>
    /// Visual projectile: implement IProjectileView cho SkillSystem.
    /// Vị trí logic do CastProjectileAction tính (trajectory), visual lerp giữa frame.
    /// </summary>
    public class Projectile : MonoBehaviour, IProjectileView
    {
        [Header("Components")]
        [SerializeField] private Transform positionPivot;
        [SerializeField] private Transform scalePivot;
        [SerializeField] private Transform rotatePivot;
        [SerializeField] private TrailRenderer trailRenderer;
        [SerializeField] private ColliderData[] colliders = new ColliderData[0];

        public ColliderData[] Colliders { get; private set; }

        private Vector3 targetPosition;
        private Vector3 previousPosition;
        private float deltaTime;
        private float elapsedTime;
        private bool isRunning;
        private bool shouldDestroy;

        public void Initialize(SkillRuntimeData runtimeData)
        {
            isRunning = true;
            shouldDestroy = false;
            targetPosition = previousPosition = positionPivot.position;
            EnableTrail();
        }

        public void ImmediatelySetPosition(Vector3 position)
        {
            positionPivot.position = position;
        }

        public void SetPosition(Vector2 position, Vector2 direction, float deltaTime)
        {
            this.previousPosition = positionPivot.position;
            this.targetPosition = position;
            this.deltaTime = deltaTime;
            this.elapsedTime = 0;

            Rotate(direction);
        }

        public void Rotate(Vector2 direction)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            if (rotatePivot != null)
                rotatePivot.localEulerAngles = new Vector3(0, 0, angle);
        }

        public void SetSizeScale(float scale)
        {
            Colliders = new ColliderData[colliders.Length];

            for (int i = 0; i < colliders.Length; i++)
            {
                ColliderData colliderData = colliders[i];

                colliderData.circleRadius *= scale;

                colliderData.rectangleSize *= scale;

                colliderData.relativePosition *= scale;

                Colliders[i] = colliderData;
            }

            if (scalePivot != null)
                scalePivot.localScale = Vector3.one * scale;

            if (trailRenderer != null)
                trailRenderer.widthMultiplier = scale;
        }

        public void Destroy()
        {
            if (shouldDestroy) return;

            shouldDestroy = true;
        }

        public void Active()
        {
            gameObject.SetActive(true);
        }

        private void Update()
        {
            if (!isRunning) return;

            elapsedTime += Time.deltaTime;

            float t = Mathf.Clamp01(elapsedTime / Mathf.Max(0.0001f, deltaTime));

            positionPivot.position = Vector3.Lerp(previousPosition, targetPosition, t);

            if (t >= 1f && shouldDestroy)
            {
                isRunning = false;

                DisableTrail();

                Pool.Destroy(gameObject);
            }
        }

        private void EnableTrail()
        {
            if (trailRenderer != null)
            {
                trailRenderer.emitting = true;
                trailRenderer.enabled = true;
            }
        }

        private void DisableTrail()
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
