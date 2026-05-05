using UnityEngine;

namespace _KITSystem.SkillSystem.Runtime
{
    internal class BulletTrajectoryAction : BaseTrajectoryAction
    {
        float initialSpeed;
        float acceleration;
        private float travel;

        public BulletTrajectoryAction(float initialSpeed, float acceleration, Vector3 start, Vector3 goal) : base(start, goal)
        {
            this.initialSpeed = initialSpeed;
            this.acceleration = acceleration;
        }

        protected override Vector3 OnEvaluatePosition(Vector3 start, Vector3 goal, Vector3 direction, float deltaTime)
        {
            float d = 0.5f * acceleration * deltaTime * deltaTime + initialSpeed * deltaTime;
            float s = d - travel;
            travel = d;
            return start + s * direction;
        }
    }
}