using _KITSystem.SkillSystem.Core;
using UnityEngine;
using UnityEngine.Splines;

namespace _KITSystem.SkillSystem.Imp
{
    public class SplineTrajectory : BaseTrajectory
    {
        private Spline spline;
        private float windupDuration;
        private float executeDuration;
        private float recoveryDuration;
        private Phase phase = Phase.Windup;
        private Vector2 startSplinePosition;
        private Vector2 endSplinePosition;
        private float elapsedTime;

        public SplineTrajectory(Spline spline,
            float windupDuration, float executeDuration, float recoveryDuration,
            Vector2 start, Vector2 goal) : base(start, goal)
        {
            this.spline = spline;
            this.windupDuration = windupDuration;
            this.executeDuration = executeDuration;
            this.recoveryDuration = recoveryDuration;
            this.startSplinePosition = (Vector3)spline.EvaluatePosition(0);
            this.startSplinePosition = (Vector3)spline.EvaluatePosition(1);
        }

        public override Vector2 EvaluatePosition(float deltaTime)
        {
            elapsedTime += deltaTime;

            float f;
            
            switch (phase)
            {
                case Phase.Windup:
                    f = Mathf.Clamp01(elapsedTime / elapsedTime);
                    Vector2 position = Vector2.Lerp(Start, startSplinePosition, f);
                    break;
                case Phase.Execute:
                    break;
                case Phase.Recovery:
                    break;
            }
            
            return Vector2.zero;
        }

        public override Vector2 EvaluateDirection(float deltaTime)
        {
            return Vector2.zero;
        }
        
        public enum Phase
        {
            Windup, Execute, Recovery, Complete
        }
    }
}