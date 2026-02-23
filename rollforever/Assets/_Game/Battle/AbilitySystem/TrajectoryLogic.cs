using System;
using Unity.Mathematics;

namespace _Game.Battle.AbilitySystem
{
    public sealed class TrajectoryLogic : IDisposable
    {
        private readonly ISubTrajectory trajectory;

        public TrajectoryLogic(TrajectoryArg arg)
        {
            switch (arg.type)
            {
                case TrajectoryType.Teleport:
                    trajectory = new TeleportSubTrajectory();
                    break;
                case TrajectoryType.Velocity:
                    trajectory = new VelocitySubTrajectory(arg.velocity.acceleration, arg.velocity.speed);
                    break;
                default:
#if DEVELOP_MODE
                    throw new Exception($"Trajectory {arg.type} chưa được xác định");     
#endif
                    trajectory = new TeleportSubTrajectory();
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