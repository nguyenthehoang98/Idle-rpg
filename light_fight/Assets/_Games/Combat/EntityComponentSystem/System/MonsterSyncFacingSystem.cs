using _Games.Combat.EntityComponentSystem.Data;
using _Games.Combat.EntityComponentSystem.Model;
using ProjectDawn.Navigation;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace _Games.Combat.EntityComponentSystem.System
{
    [BurstCompile]
    [UpdateInGroup(typeof(BattleSimulationGroup))]
    [RequireMatchingQueriesForUpdate]
    public partial struct MonsterSyncFacingSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Dependency = new FlipFacingJob().ScheduleParallel(state.Dependency);
            state.Dependency.Complete();
            
            foreach (var (flip, transform, entity) in SystemAPI
                         .Query<RefRW<MonsterFlipData>, RefRW<LocalTransform>>()
                         .WithAll<MonsterTag>()
                         .WithNone<MonsterDeadTag, MonsterBlockMovementTag>()
                         .WithEntityAccess())
            {
                MonsterFlipData flipData = flip.ValueRO;
                if (flipData.Changed)
                {
                    float angle = flipData.FacingRight ? 0 : 180;
                    LocalTransform t = transform.ValueRW;
                    t.Rotation = quaternion.RotateY(math.radians(angle));
                    transform.ValueRW = t;
                    
                    flipData.Changed = false;
                    flip.ValueRW = flipData;
                }
            }
        }

        [BurstCompile]
        [WithNone(typeof(MonsterDeadTag), typeof(MonsterBlockMovementTag))]
        partial struct FlipFacingJob : IJobEntity
        {
            public void Execute(in Entity entity, in MonsterTag tag, [ReadOnly] in AgentBody body,
                ref MonsterFlipData flip)
            {
                if (math.lengthsq(body.Velocity.xy) < 0.5f) return;
                bool facing = body.Velocity.x < 0;
                if (flip.FacingRight != facing)
                {
                    flip.FacingRight = facing;
                    flip.Changed = true;
                }
            }
        }
    }
}