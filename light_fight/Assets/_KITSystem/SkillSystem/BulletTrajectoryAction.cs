using _KITSystem.SkillSystem.Model;
using Unity.Mathematics;
using UnityEngine;

namespace _KITSystem.SkillSystem
{
    internal class BulletTrajectoryAction : BaseTrajectoryAction
    {
        private readonly float initialSpeed;
        private readonly float acceleration;
        
        private float elapsedTime;

        public BulletTrajectoryAction(float initialSpeed, float acceleration, float2 start, float2 goal) : base(start, goal)
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