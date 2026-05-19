using Unity.Mathematics;

namespace _KITSystem.SkillSystem.Runtime
{
    internal class BulletTrajectoryAction : BaseTrajectoryAction
    {
        float initialSpeed;
        float acceleration;
        private float elapsedTime;

        public BulletTrajectoryAction(float initialSpeed, float acceleration, float2 start, float2 goal) :
            base(start, goal)
        {
            this.initialSpeed = initialSpeed;
            this.acceleration = acceleration;
        }

        protected override float2 OnEvaluatePosition(float deltaTime)
        {
            elapsedTime += deltaTime;

            float d = 0.5f * acceleration * elapsedTime * elapsedTime + initialSpeed * elapsedTime;
            return start + d * direction;
        }
    }
}