using System;
using Unity.Mathematics;

namespace _Game.AbilitySystem
{
    public sealed class TrajectoryLogic : IDisposable
    {
        private readonly ISubTrajectory trajectory;

        public TrajectoryLogic(TrajectoryArg arg)
        {
            switch (arg.type)
            {
                case TrajectoryType.Velocity:
                    trajectory = new VelocitySubTrajectory(arg.velocity.acceleration, arg.velocity.speed);
                    break;
                default:
#if UNITY_EDITOR
                    string message = "Not define TrajectoryType: " + arg.type;
                    throw new NotImplementedException(message);     
#endif
                    break;
            }
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
        float acceleration;
        float speed;
        float2 startPos;
        float2 direction;
        
        float elapsed;
        
        public VelocitySubTrajectory(float acceleration, float speed)
        {
            this.acceleration = acceleration;
            this.speed = speed;
        }
        
        public void Init(float2 startPos, float2 endPos)
        {
            this.startPos = startPos;
            this.direction = math.normalize(endPos - startPos);
        }

        public float2 Execute(float dt)
        {
            elapsed += dt;
            float f = 0.5f * acceleration * elapsed * elapsed + speed * elapsed;
            return f * direction + startPos;
        }
    }
}