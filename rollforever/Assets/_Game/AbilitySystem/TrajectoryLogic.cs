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
                case TrajectoryType.None:
                    trajectory = new NoneSubTrajectory();
                    break;
                case TrajectoryType.Velocity:
                    trajectory = new VelocitySubTrajectory(arg.velocity.acceleration, arg.velocity.speed);
                    break;
                default:
#if UNITY_EDITOR
                    throw new Exception("Not define TrajectoryType: " + arg.type);     
#endif
                    trajectory = new NoneSubTrajectory();
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
}