using _Games.Combat.EntityComponentSystem.Data;
using ProjectDawn.Navigation;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace _Games.Combat.EntityComponentSystem.System
{
    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    public partial struct ProjectileOverlapSystem : ISystem
    {
        ComponentLookup<ProjectileSkillData> projectileSkillDataLookup;
        ComponentLookup<LocalTransform> localTransformLookup;
        BufferLookup<CircleBuffer> circleBufferLookup;
        BufferLookup<CollisionBuffer> collisionBufferLookup;

        public void OnCreate(ref SystemState state)
        {
            collisionBufferLookup = state.GetBufferLookup<CollisionBuffer>();
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
            collisionBufferLookup.Update(ref state);
            
            var handle = state.WorldUnmanaged.GetExistingUnmanagedSystem<SpatialGridSystem>();
            ref var gridSystem = ref state.WorldUnmanaged.GetUnsafeSystemRef<SpatialGridSystem>(handle);
            var grid = gridSystem.Grid;
            
            var job = new CircleOverlapJob
            {
                Grid = grid,
                LocalTransformLookup = localTransformLookup,
                CircleBufferLookup = circleBufferLookup,
                CollisionBufferLookup = collisionBufferLookup,
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
            [NativeDisableParallelForRestriction]
            public BufferLookup<CollisionBuffer> CollisionBufferLookup;

            public void Execute(in Entity entity, in LocalTransform transform, ref ProjectileSkillData skillData)
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
                    int2 maxCell = (int2)math.floor(max);
                    int capacity = math.abs(maxCell.x - minCell.x + 1) * math.abs(maxCell.y - minCell.y + 1);
                    NativeParallelHashSet<Entity> temps = new NativeParallelHashSet<Entity>(capacity, Allocator.Temp);
                    NativeParallelHashSet<Entity> copy;
                    if (CollisionBufferLookup.HasBuffer(entity))
                    {
                        DynamicBuffer<CollisionBuffer> collections = CollisionBufferLookup[entity];
                        copy = new NativeParallelHashSet<Entity>(collections.Capacity, Allocator.Temp);
                        foreach (var monster in collections)
                        {
                            copy.Add(monster.Entity);
                        }
                    }
                    else
                    {
                        copy = new NativeParallelHashSet<Entity>(capacity, Allocator.Temp);
                    }

                    for (int x = minCell.x; x <= maxCell.x; x++)
                    {
                        for (int y = minCell.y; y <= maxCell.y; y++)
                        {
                            int2 cell = new int2(x, y);
                            if (Grid.TryGetFirstValue(cell, out var monster, out var iterator))
                            {
                                do
                                {
                                    if (!LocalTransformLookup.HasComponent(monster))
                                        continue;
                                    if (!CircleBufferLookup.TryGetBuffer(monster, out DynamicBuffer<CircleBuffer> otherBuffers))
                                        continue;

                                    float2 monsterPosition = LocalTransformLookup[monster].Position.xy;
                                    foreach (var circleBuffer in otherBuffers)
                                    {
                                        float bufferRadius = circleBuffer.Radius;
                                        float totalRadius = bufferRadius + radius;
                                        float distanceSq = math.distancesq(monsterPosition, position);
                                        if (distanceSq <= totalRadius * totalRadius)
                                        {
                                            temps.Add(monster);
                                            break;
                                        }
                                    }
                                } while (Grid.TryGetNextValue(out monster, ref iterator));
                            }
                        }
                    }
                    
                    foreach (var monster in temps)
                    {
                        if (copy.Add(monster))
                        {
                            skillData.TotalUnitBeHit++;
                            CollisionBufferLookup[entity].Add(new CollisionBuffer(monster));
                        }
                    }

                    copy.Dispose();
                    temps.Dispose();
                }
            }
        }
    }
}