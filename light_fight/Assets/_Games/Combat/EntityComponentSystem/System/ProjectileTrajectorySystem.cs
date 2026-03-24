using _Games.Combat.EntityComponentSystem.Data;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace _Games.Combat.EntityComponentSystem.System
{
    [BurstCompile]
    public partial struct ProjectileTrajectorySystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Dependency = new ProjectileCurveJob
            {
                DeltaTime = state.WorldUnmanaged.Time.DeltaTime
            }.ScheduleParallel(state.Dependency);
        }
        
        [BurstCompile]
        partial struct ProjectileCurveJob : IJobEntity
        {
            public double DeltaTime;

            public void Execute(ref ProjectileCurveTrajectory curve,
                ref ProjectileTrajectory trajectory, ref LocalTransform transform)
            {
                trajectory.ElapsedTime += DeltaTime;
                float t = (float)trajectory.ElapsedTime;
                double d = curve.Evaluate(t);
                float3 position = (float)d * trajectory.Direction + trajectory.StartPosition;
                transform.Position = position;
            }
        }
    }
}