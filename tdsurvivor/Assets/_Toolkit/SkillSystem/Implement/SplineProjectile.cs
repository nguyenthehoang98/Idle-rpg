using System;
using _Toolkit.SkillSystem.Core;
using UnityEngine;
using UnityEngine.Splines;

namespace _Toolkit.SkillSystem.Implement
{
    public class SplineProjectile : Projectile
    {
        [SerializeField] private Spline spline;
        [SerializeField] private Transform rotateTransform;

        public event Action<ProjectilePhase> OnPhaseChanged;

        float startDuration;
        float duration;
        float endDuration;

        ProjectilePhase phase;
        float elapsedTime;

        Vector3 startSplinePosition;
        Vector3 endSplinePosition;

        float currentAngle;
        float newAngle;
        
        public override void EnsureValid(out float lifetime)
        {
            lifetime = Context.StartDuration + Context.Duration + Context.EndDuration;
        }

        protected override void OnStartup()
        {
            base.OnStartup();

            startDuration = Context.StartDuration;
            duration = Context.Duration;
            endDuration = Context.EndDuration;

            phase = ProjectilePhase.Undefined;
            elapsedTime = 0;

            currentAngle = rotateTransform.localEulerAngles.z;
            newAngle = Mathf.Atan2(Direction.y, Direction.x) * Mathf.Rad2Deg;
            
            startSplinePosition = Validate(Start, spline.EvaluatePosition(0), Direction);
            endSplinePosition = Validate(Start, spline.EvaluatePosition(1), Direction);
        }

        protected override void OnTick(float deltaTime)
        {
            if (phase == ProjectilePhase.Undefined)
            {
                phase = ProjectilePhase.Start;
                elapsedTime = 0;
                OnPhaseChanged?.Invoke(phase);
            }

            TransitionPreviousPosition = transform.position;
            
            elapsedTime += deltaTime;

            float progress = 0;

            switch (phase)
            {
                case ProjectilePhase.Start:
                    progress = startDuration <= 0 ? 1 : Mathf.Clamp01(elapsedTime / startDuration);

                    if (startDuration > 0 && elapsedTime >= startDuration)
                    {
                        phase = ProjectilePhase.Execute;
                        elapsedTime = 0;
                        OnPhaseChanged?.Invoke(phase);
                    }
                    else TransitionDeltaPosition = Vector3.Lerp(Start, startSplinePosition, progress);

                    break;
                case ProjectilePhase.Execute:
                    progress = duration <= 0 ? 1 : Mathf.Clamp01(elapsedTime / duration);

                    if (duration > 0 && elapsedTime >= duration)
                    {
                        phase = ProjectilePhase.End;
                        elapsedTime = 0;
                        OnPhaseChanged?.Invoke(phase);
                    }
                    else TransitionDeltaPosition = Validate(Start, spline.EvaluatePosition(progress), Direction);

                    break;
                case ProjectilePhase.End:
                    progress = endDuration <= 0 ? 1 : Mathf.Clamp01(elapsedTime / endDuration);

                    if (endDuration > 0 && elapsedTime >= endDuration)
                    {
                        phase = ProjectilePhase.Complete;
                        elapsedTime = 0;
                        OnPhaseChanged?.Invoke(phase);
                    }
                    else TransitionDeltaPosition = Vector3.Lerp(endSplinePosition, endSplinePosition, progress);

                    break;
            }

            Vector3 direction = TransitionDeltaPosition - TransitionPreviousPosition;
            newAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            
            currentAngle = rotateTransform.localEulerAngles.z;

            if (phase == ProjectilePhase.Complete) Shutdown();
        }

        private void LateUpdate()
        {
            if (!IsDestroyed)
            {
                float t = Mathf.Clamp01(FixedElapsedTime / EngineDeltaTime);
                float angle = Mathf.Lerp(currentAngle, newAngle, t);
                rotateTransform.localEulerAngles = new Vector3(0, 0, angle);
            }
        }

        protected override float TransitionDuration => startDuration + duration + endDuration;
        protected override Vector3 TransitionDeltaPosition { get; set; }
        protected override Vector3 TransitionPreviousPosition { get; set; }

        private static Vector3 Validate(Vector3 center, Vector3 localPosition, Vector3 direction)
        {
            Matrix4x4 matrix = Matrix4x4.TRS(center,
                Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg),
                Vector3.one
            );
            return matrix.MultiplyPoint3x4(localPosition);
        }
    }
}