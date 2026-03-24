using _Games.Combat.EntityComponentSystem.Data;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace _Games.Combat.EntityComponentSystem.System
{
    [BurstCompile]
    [UpdateAfter(typeof(SpatialGridSystem))]
    public partial struct ProjectileOverlapSystem : ISystem
    {
        ComponentLookup<ProjectileSkillData> projectileSkillDataLookup;
        ComponentLookup<LocalTransform> localTransformLookup;
        BufferLookup<CircleBuffer> circleBufferLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            localTransformLookup = state.GetComponentLookup<LocalTransform>();
            circleBufferLookup = state.GetBufferLookup<CircleBuffer>();
            projectileSkillDataLookup = state.GetComponentLookup<ProjectileSkillData>();
        }
        
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            localTransformLookup.Update(ref state);
            circleBufferLookup.Update(ref state);
            projectileSkillDataLookup.Update(ref state);
            
            var handle = state.WorldUnmanaged.GetExistingUnmanagedSystem<SpatialGridSystem>();
            ref var gridSystem = ref state.WorldUnmanaged.GetUnsafeSystemRef<SpatialGridSystem>(handle);
            var grid = gridSystem.Grid;
            
            var job = new CircleOverlapJob
            {
                Grid = grid,
                LocalTransformLookup = localTransformLookup,
                CircleBufferLookup = circleBufferLookup,
            };
            state.Dependency = job.ScheduleParallel(state.Dependency);
            state.CompleteDependency();
        }
        
        [BurstCompile]
        partial struct CircleOverlapJob : IJobEntity
        {
            [ReadOnly] public NativeParallelMultiHashMap<int2, Entity> Grid;
            [ReadOnly] public ComponentLookup<LocalTransform> LocalTransformLookup;
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
    }
}