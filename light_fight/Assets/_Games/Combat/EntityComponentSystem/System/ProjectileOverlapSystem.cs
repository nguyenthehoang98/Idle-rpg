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
    [UpdateAfter(typeof(SpatialGridSystem))]
    [UpdateInGroup(typeof(CustomSimulationGroup))]
    [RequireMatchingQueriesForUpdate]
    public partial struct ProjectileOverlapSystem : ISystem
    {
        ComponentLookup<ProjectileSkillData> projectileSkillDataLookup;
        ComponentLookup<LocalTransform> localTransformLookup;
        ComponentLookup<PlayerTag> playerTagLookup;
        ComponentLookup<MonsterTag> monsterTagLookup;
        BufferLookup<CircleBuffer> circleBufferLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            monsterTagLookup = state.GetComponentLookup<MonsterTag>();
            playerTagLookup = state.GetComponentLookup<PlayerTag>();
            localTransformLookup = state.GetComponentLookup<LocalTransform>();
            circleBufferLookup = state.GetBufferLookup<CircleBuffer>();
            projectileSkillDataLookup = state.GetComponentLookup<ProjectileSkillData>();
        }
        
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            monsterTagLookup.Update(ref state);
            playerTagLookup.Update(ref state);
            localTransformLookup.Update(ref state);
            circleBufferLookup.Update(ref state);
            projectileSkillDataLookup.Update(ref state);
            
            var handle = state.WorldUnmanaged.GetExistingUnmanagedSystem<SpatialGridSystem>();
            ref var gridSystem = ref state.WorldUnmanaged.GetUnsafeSystemRef<SpatialGridSystem>(handle);
            var grid = gridSystem.Grid;
            
            state.Dependency = new CircleOverlapJob
            {
                Grid = grid,
                LocalTransformLookup = localTransformLookup,
                CircleBufferLookup = circleBufferLookup,
                PlayerTagLookup = playerTagLookup,
                MonsterTagLookup = monsterTagLookup,
            }.ScheduleParallel(state.Dependency);
            state.CompleteDependency();
            
            state.Dependency = new ProjectileExplosionJob
            {
                Grid = grid,
                LocalTransformLookup = localTransformLookup,
                CircleBufferLookup = circleBufferLookup,
                PlayerTagLookup = playerTagLookup,
                MonsterTagLookup = monsterTagLookup,
                DeltaTime = state.WorldUnmanaged.Time.DeltaTime,
            }.ScheduleParallel(state.Dependency);
            state.CompleteDependency();
        }
        
        [BurstCompile]
        partial struct CircleOverlapJob : IJobEntity
        {
            [ReadOnly] public NativeParallelMultiHashMap<int2, Entity> Grid;
            [ReadOnly] public ComponentLookup<LocalTransform> LocalTransformLookup;
            [ReadOnly] public ComponentLookup<PlayerTag> PlayerTagLookup;
            [ReadOnly] public ComponentLookup<MonsterTag> MonsterTagLookup;
            [NativeDisableParallelForRestriction]
            public BufferLookup<CircleBuffer> CircleBufferLookup;

            public void Execute(in Entity entity, in LocalTransform transform,
                ref DynamicBuffer<CollisionBuffer> collisionBuffers, ref ProjectileSkillData skillData)
            {
                if (!CircleBufferLookup.TryGetBuffer(entity, out DynamicBuffer<CircleBuffer> buffers))
                    return;
                for (int i = 0; i < buffers.Length; i++)
                {
                    CircleBuffer buffer = buffers[i];
                    float radius = buffer.Radius;
                    float2 position = transform.Position.xy + buffer.Offset.xy;
                    float2 min = position - radius;
                    float2 max = position + radius;
                    int2 minCell = (int2)math.floor(min);
                    int2 maxCell = (int2)math.ceil(max);
                    NativeParallelHashSet<Entity> copy = new NativeParallelHashSet<Entity>(collisionBuffers.Capacity, Allocator.Temp);
                    foreach (var collisionBuffer in collisionBuffers)
                    {
                        copy.Add(collisionBuffer.Entity);
                    }

                    for (int x = minCell.x; x <= maxCell.x; x++)
                    {
                        for (int y = minCell.y; y <= maxCell.y; y++)
                        {
                            int2 cell = new int2(x, y);
                            if (Grid.TryGetFirstValue(cell, out var unit, out var iterator))
                            {
                                do
                                {
                                    if (!LocalTransformLookup.HasComponent(unit))
                                        continue;
                                    if (!CircleBufferLookup.TryGetBuffer(unit, out DynamicBuffer<CircleBuffer> otherBuffers))
                                        continue;
                                    if (skillData.IsSourcePlayer() && PlayerTagLookup.HasComponent(unit))
                                        continue;
                                    if (skillData.IsSourceMonster() && MonsterTagLookup.HasComponent(unit))
                                        continue;
                                    float2 monsterPosition = LocalTransformLookup[unit].Position.xy;
                                    foreach (var circleBuffer in otherBuffers)
                                    {
                                        float bufferRadius = circleBuffer.Radius;
                                        float totalRadius = bufferRadius + radius;
                                        float distanceSq = math.distancesq(monsterPosition, position);
                                        if (distanceSq <= totalRadius * totalRadius)
                                        {
                                            if (copy.Add(unit) && skillData.TotalUnitBeHit < skillData.MaximumHits)
                                            {
                                                skillData.TotalUnitBeHit++;
                                                collisionBuffers.Add(new CollisionBuffer(unit));                                                
                                                break;
                                            }
                                        }
                                    }
                                } while (Grid.TryGetNextValue(out unit, ref iterator));
                            }
                        }
                    }

                    copy.Dispose();
                }
            }
        }
        
         [BurstCompile]
        partial struct ProjectileExplosionJob : IJobEntity
        {
            [ReadOnly] public float DeltaTime;
            [ReadOnly] public NativeParallelMultiHashMap<int2, Entity> Grid;
            [ReadOnly] public ComponentLookup<LocalTransform> LocalTransformLookup;
            [ReadOnly] public ComponentLookup<PlayerTag> PlayerTagLookup;
            [ReadOnly] public ComponentLookup<MonsterTag> MonsterTagLookup;
            [NativeDisableParallelForRestriction]
            public BufferLookup<CircleBuffer> CircleBufferLookup;

            void Execute(in Entity entity, in LocalTransform transform, in ProjectileTrajectory trajectory,
                ref ProjectileExplosion explosion, ref ProjectileSkillData skillData,
                ref DynamicBuffer<CollisionBuffer> buffers)
            {
                explosion.ElapsedTime += DeltaTime;
                if (buffers.Length == 0 || explosion.OnTriggerModifier || explosion.ElapsedTime < explosion.Duration) return;

                explosion.OnTriggerModifier = true;
                float radius = explosion.Radius;
                float3 offset = trajectory.Direction * explosion.OffsetCenter;
                float2 position = transform.Position.xy + new float2(offset.x, offset.y);
                float2 min = position - radius;
                float2 max = position + radius;
                int2 minCell = (int2)math.floor(min);
                int2 maxCell = (int2)math.floor(max);

                NativeParallelHashSet<Entity> copy = new NativeParallelHashSet<Entity>(buffers.Capacity, Allocator.Temp);
                foreach (var collisionBuffer in buffers)
                {
                    copy.Add(collisionBuffer.Entity);
                }

                for (int x = minCell.x; x <= maxCell.x; x++)
                {
                    for (int y = minCell.y; y <= maxCell.y; y++)
                    {
                        int2 cell = new int2(x, y);
                        if (Grid.TryGetFirstValue(cell, out var unit, out var iterator))
                        {
                            do
                            {
                                if (!LocalTransformLookup.HasComponent(unit))
                                    continue;
                                if(!CircleBufferLookup.TryGetBuffer(unit, out DynamicBuffer<CircleBuffer> otherBuffers)) 
                                    continue;
                                if (skillData.IsSourcePlayer() && PlayerTagLookup.HasComponent(unit))
                                    continue;
                                if (skillData.IsSourceMonster() && MonsterTagLookup.HasComponent(unit))
                                    continue;
                                float2 monsterPosition = LocalTransformLookup[unit].Position.xy;
                                foreach (var circleBuffer in otherBuffers)
                                {
                                    float bufferRadius = circleBuffer.Radius;
                                    float totalRadius = bufferRadius + radius;
                                    float distanceSq = math.distancesq(monsterPosition, position);
                                    if (distanceSq <= totalRadius * totalRadius)
                                    {
                                        if (copy.Add(unit))
                                        {
                                            skillData.TotalUnitBeHit++;
                                            buffers.Add(new CollisionBuffer(unit));
                                            break;
                                        }
                                    }
                                }
                            } while (Grid.TryGetNextValue(out unit, ref iterator));
                        }
                    }
                }

                copy.Dispose();

                // todo: force destroy
                skillData.ElapsedLifeTime = skillData.LifeTime;
            }
        }
    }
}