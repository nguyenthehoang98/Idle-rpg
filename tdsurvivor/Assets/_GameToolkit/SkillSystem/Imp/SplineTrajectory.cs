using System;
using _GameToolkit.SkillSystem.Core;
using _GameToolkit.SkillSystem.Utils;
using UnityEngine;
using UnityEngine.Splines;

namespace _GameToolkit.SkillSystem.Imp
{
    /// <summary>
    /// Trajectory bay theo spline với 3 phase: Windup → Execute → Recovery.
    /// Port từ Recovery 2 (_KITSystem.SkillSystem.Imp.SplineTrajectory).
    /// </summary>
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
        private float totalElapsedTime;

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
            this.endSplinePosition = ValidatePosition(start, spline.EvaluatePosition(1), d);
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
            totalElapsedTime += deltaTime;

            Vector2 prevPosition = deltaPosition;

            float p;

            switch (phase)
            {
                case Phase.Windup:
                    p = windupDuration <= 0f ? 1f : Mathf.Clamp01(elapsedTime / windupDuration);

                    if (windupDuration > 0f && elapsedTime >= windupDuration)
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

                    p = executeDuration <= 0f ? 1f : Mathf.Clamp01(elapsedTime / executeDuration);

                    if (executeDuration > 0f && elapsedTime >= executeDuration)
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

                    p = recoveryDuration <= 0f ? 1f : Mathf.Clamp01(elapsedTime / recoveryDuration);

                    if (recoveryDuration > 0f && elapsedTime >= recoveryDuration)
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

#if UNITY_EDITOR
            float t = windupDuration + executeDuration + recoveryDuration;

            GizmosLine.Line(prevPosition, deltaPosition, Color.yellow, t - totalElapsedTime);
#endif

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
            Matrix4x4 matrix = Matrix4x4.TRS(
                center,
                Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg),
                Vector3.one);

            return matrix.MultiplyPoint3x4(localPosition);
        }
    }
}
