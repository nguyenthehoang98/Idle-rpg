using UnityEngine;

namespace _KITSystem.SkillSystem.Runtime
{
    internal class BulletTrajectoryAction : BaseTrajectoryAction
    {
        float initialSpeed;
        float acceleration;
        private float elapsedTime;

        public BulletTrajectoryAction(float initialSpeed, float acceleration, Vector3 start, Vector3 goal) :
            base(start, goal)
        {
            this.initialSpeed = initialSpeed;
            this.acceleration = acceleration;
        }

        protected override Vector3 OnEvaluatePosition(float deltaTime)
        {
            elapsedTime += deltaTime;

            float d = 0.5f * acceleration * elapsedTime * elapsedTime + initialSpeed * elapsedTime;
            return start + d * direction;
        }
    }
}