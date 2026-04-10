using _Games.Combat.EntityComponentSystem.Data;
using ProjectDawn.Custom;
using ProjectDawn.Navigation;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace _Games.Combat.EntityComponentSystem.System
{
    [BurstCompile]
    [UpdateInGroup(typeof(CustomSimulationGroup))]
    [RequireMatchingQueriesForUpdate]
    public partial struct MonsterSyncFacingSystem : ISystem
    {
        private float rad0;
        private float rad180;

        public void OnCreate(ref SystemState state)
        {
            rad0 = math.radians(0);
            rad180 = math.radians(180);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Dependency = new FlipFacingJob
            {
                rad0 = rad0,
                rad180 = rad180
            }.Schedule(state.Dependency);
        }

        [BurstCompile]
        [WithAll(typeof(MonsterTag))]
        [WithNone(typeof(MonsterDeadTag), typeof(MonsterBlockMovementTag))]
        partial struct FlipFacingJob : IJobEntity
        {
            public float rad0;
            public float rad180;
            
            public void Execute(ref LocalTransform transform)
            { 
                float rad = transform.Position.x < 0 ? rad180 : rad0;
                transform.Rotation = quaternion.RotateY(rad);
            }
        }
    }
}