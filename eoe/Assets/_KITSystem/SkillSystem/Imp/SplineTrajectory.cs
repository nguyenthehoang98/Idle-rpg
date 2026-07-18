using System;
using _KITSystem.SkillSystem.Core;
using UnityEngine;
using UnityEngine.Splines;

namespace _KITSystem.SkillSystem.Imp
{
    public class SplineTrajectory : BaseTrajectory
    {
        private readonly Spline spline;
        private readonly float windupDuration;
        private readonly float executeDuration;
        private readonly float recoveryDuration;
        private readonly Vector2 startSplinePosition;
        private readonly Vector2 endSplinePosition;

        public event Action<Phase> OnChangePhase;

        private Phase phase = Phase.Undefined;
        private Vector2 direction;
        private Vector2 deltaPosition;
        private float elapsedTime;

        public SplineTrajectory(Spline spline,
            float windupDuration, float executeDuration, float recoveryDuration,
            Vector2 start, Vector2 goal) : base(start, goal)
        {
            this.spline = spline;
            this.windupDuration = windupDuration;
            this.executeDuration = executeDuration;
            this.recoveryDuration = recoveryDuration;

            Vector2 d = (goal - start).normalized;
            this.startSplinePosition = ValidatePosition(start, spline.EvaluatePosition(0), d);
            this.startSplinePosition = ValidatePosition(start, spline.EvaluatePosition(1), d);
        }

        public override Vector2 EvaluatePosition(float deltaTime)
        {
            if (phase == Phase.Undefined)
            {
                phase = Phase.Windup;

                elapsedTime = 0f;

                OnChangePhase?.Invoke(phase);
            }

            elapsedTime += deltaTime;

            Vector2 prevPosition = deltaPosition;

            float p;
            float d;

            switch (phase)
            {
                case Phase.Windup:
                    d = windupDuration - deltaTime;

                    p = windupDuration <= 0f ? 1f : Mathf.Clamp01(elapsedTime / d);

                    if (elapsedTime >= d)
                    {
                        elapsedTime = 0;

                        phase = Phase.Execute;

                        OnChangePhase?.Invoke(phase);
                    }
                    else
                    {
                        deltaPosition = Vector2.Lerp(Start, startSplinePosition, p);
                    }

                    break;
                case Phase.Execute:

                    d = executeDuration - elapsedTime;

                    p = executeDuration <= 0f ? 1f : Mathf.Clamp01(elapsedTime / d);

                    if (elapsedTime >= d)
                    {
                        elapsedTime = 0;

                        phase = Phase.Recovery;

                        OnChangePhase?.Invoke(phase);
                    }
                    else
                    {
                        deltaPosition = ValidatePosition(Start, spline.EvaluatePosition(p), Direction);
                    }

                    break;
                case Phase.Recovery:

                    d = recoveryDuration - deltaTime;

                    p = recoveryDuration <= 0f ? 1f : Mathf.Clamp01(elapsedTime / d);

                    if (elapsedTime >= d)
                    {
                        elapsedTime = 0;

                        phase = Phase.Complete;

                        OnChangePhase?.Invoke(phase);
                    }
                    else
                    {
                        deltaPosition = Vector2.Lerp(endSplinePosition, Start, p);
                    }

                    break;
            }

            direction = (deltaPosition - prevPosition).normalized;

            return deltaPosition;
        }

        public override Vector2 EvaluateDirection(float deltaTime)
        {
            return direction;
        }

        public enum Phase
        {
            Undefined,
            Windup,
            Execute,
            Recovery,
            Complete
        }

        private static Vector2 ValidatePosition(Vector3 center, Vector3 localPosition, Vector2 direction)
        {
            Quaternion rot = Quaternion.FromToRotation(Vector3.right, direction.normalized);

            Vector3 rotatedPos = center + rot * localPosition;

            return rotatedPos;
        }
    }
}