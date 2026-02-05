using System;
using Unity.Mathematics;

namespace _Game.AbilitySystem
{
    public sealed class TrajectoryLogic : IDisposable
    {
        private readonly ISubTrajectory trajectory;

        public TrajectoryLogic(TrajectoryArg arg)
        {
            trajectory = new VelocitySubTrajectory(arg.velocity.acceleration, arg.velocity.speed);
        }

        public void Startup(float2 startPos, float2 endPos)
        {
            trajectory.Init(startPos, endPos);
        }

        public float2 Update(float dt)
        {
            return trajectory.Execute(dt);
        }

        public void Shutdown()
        {
            
        }
        
        public void Dispose()
        {
        }
    }

    interface ISubTrajectory
    {
        void Init(float2 startPos, float2 endPos);
        
        float2 Execute(float dt);
    }

    class VelocitySubTrajectory : ISubTrajectory
    {
        
        public float acceleration;
        public float speed;

        public VelocitySubTrajectory(float acceleration, float speed)
        {
            this.acceleration = acceleration;
            this.speed = speed;
        }
        
        public void Init(float2 startPos, float2 endPos)
        {
            
        }

        public float2 Execute(float dt)
        {
            
        }
    }
}