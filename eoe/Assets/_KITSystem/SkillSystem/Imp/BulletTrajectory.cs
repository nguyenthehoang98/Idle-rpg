using _KITSystem.SkillSystem.Core;
using UnityEngine;

namespace _KITSystem.SkillSystem.Imp
{
    public class BulletTrajectory : BaseTrajectory
    {
        private readonly float initialSpeed;
        private readonly float acceleration;

        private float elapsedTime;

        public BulletTrajectory(float initialSpeed, float acceleration, Vector2 start, Vector2 goal) : base(start, goal)
        {
            this.initialSpeed = initialSpeed;

            this.acceleration = acceleration;
        }

        protected override Vector2 OnEvaluatePosition(float deltaTime)
        {
            elapsedTime += deltaTime;

            float d = 0.5f * acceleration * elapsedTime * elapsedTime + initialSpeed * elapsedTime;

            return Start + d * Direction;
        }
    }
}