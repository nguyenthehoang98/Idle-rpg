using _KITSystem.SkillSystem.Core;
using UnityEngine;

namespace _KITSystem.SkillSystem.Imp
{
    public class ProjectileTrajectory : BaseTrajectory
    {
        private readonly AnimationCurve curve;
        private readonly float speed;
        private readonly float duration;

        private Vector2 deltaPosition;
        private float elapsedTime;
        private bool isCompleted;

        public ProjectileTrajectory(AnimationCurve curve, float speed, float duration, Vector2 start, Vector2 goal) :
            base(start, goal)
        {
            this.speed = speed;
            this.curve = curve;
            this.duration = duration;
        }

        public override Vector2 EvaluatePosition(float deltaTime)
        {
            if (!isCompleted)
            {
                elapsedTime += deltaTime;
                
                float p = Mathf.Clamp01(elapsedTime / duration);
                
                float f = curve.Evaluate(p);
                
                deltaPosition += f * Direction * deltaTime * speed;
                
                if (p >= 1f) isCompleted = true;
            }

            return deltaPosition + Start;
        }

        public override Vector2 EvaluateDirection(float deltaTime)
        {
            return Direction;
        }
    }
}