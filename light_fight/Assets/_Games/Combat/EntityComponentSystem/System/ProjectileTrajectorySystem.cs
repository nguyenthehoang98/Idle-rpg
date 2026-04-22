using _Games.Combat.EntityComponentSystem.Data;
using ProjectDawn.Custom;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace _Games.Combat.EntityComponentSystem.System
{
    [BurstCompile]
    [UpdateInGroup(typeof(CustomSimulationGroup))]
    [RequireMatchingQueriesForUpdate]
    public partial struct ProjectileTrajectorySystem : ISystem
    {
        ComponentLookup<LocalTransform> localTransformLookup;
        
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
            localTransformLookup = state.GetComponentLookup<LocalTransform>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            localTransformLookup.Update(ref state);
            
            state.Dependency = new ProjectileCurveJob
            {
                DeltaTime = state.WorldUnmanaged.Time.DeltaTime
            }.ScheduleParallel(state.Dependency);
            state.Dependency = new ProjectileParabolicJob
            {
                LocalTransformLookup = localTransformLookup,
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
                double d = curve.DistanceEvaluate(t);
                float3 position = (float)d * trajectory.Direction + trajectory.StartPosition;
                transform.Position = position;
            }
        }
        
        [BurstCompile]
        partial struct ProjectileParabolicJob : IJobEntity
        {
            [NativeDisableParallelForRestriction]
            public ComponentLookup<LocalTransform> LocalTransformLookup;
            public double DeltaTime;

            public void Execute(Entity entity, ref ProjectileParabolicTrajectory parabolic,
                ref ProjectileTrajectory trajectory)
            {
                trajectory.ElapsedTime += DeltaTime;
                float timeT = (float)math.clamp(trajectory.ElapsedTime / parabolic.Duration, 0, 1);
                float height = math.lerp(0, parabolic.MaxHeight, parabolic.HeightEvaluate(timeT));
                float3 endPosition = trajectory.EndPosition;
                if (trajectory.Target != Entity.Null && LocalTransformLookup.HasComponent(trajectory.Target))
                {
                    endPosition = LocalTransformLookup.GetRefRO(trajectory.Target).ValueRO.Position;
                }
                float3 position = math.lerp(trajectory.StartPosition, endPosition, timeT);

                RefRW<LocalTransform> rw = LocalTransformLookup.GetRefRW(entity);
                rw.ValueRW.Position = position + new float3(0, height, 0);
            }
        }
    }
}