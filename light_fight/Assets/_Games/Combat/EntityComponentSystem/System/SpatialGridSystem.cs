using _Games.Combat.EntityComponentSystem.Data;
using ProjectDawn.Navigation;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace _Games.Combat.EntityComponentSystem.System
{
    [BurstCompile]
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public partial struct SpatialGridSystem : ISystem
    {
        public NativeParallelMultiHashMap<int2, Entity> Grid;
        public EntityQuery Query;

        public void OnCreate(ref SystemState state)
        {
            Grid = new NativeParallelMultiHashMap<int2, Entity>(1024, Allocator.Persistent);
            Query = SystemAPI.QueryBuilder()
                .WithAll<MonsterTag>()
                .Build(); 
        }
        
        public void OnUpdate(ref SystemState state)
        {
            int entityCount = Query.CalculateEntityCount();
            int neededCapacity = entityCount * 16;
            if (Grid.Capacity < neededCapacity)
            {
                Grid.Capacity = math.max(neededCapacity, Grid.Capacity * 2);
            }
            
            Grid.Clear();

            state.Dependency = new PlayerCircleSpatialGridJob
            {
                Grid = Grid,
            }.Schedule(state.Dependency);
            state.Dependency.Complete();

            state.Dependency = new MonsterCircleSpatialGridJob
            {
                Grid = Grid.AsParallelWriter()
            }.ScheduleParallel(state.Dependency);
            state.Dependency.Complete();

#if UNITY_EDITOR
            foreach (var pair in Grid)
            {
                int2 cell = pair.Key;

                Vector3 p0 = new Vector3(cell.x, cell.y);
                Vector3 p1 = new Vector3(cell.x, cell.y + 1);
                Vector3 p2 = new Vector3(cell.x + 1, cell.y + 1);
                Vector3 p3 = new Vector3(cell.x + 1, cell.y);

                Debug.DrawLine(p0, p1, Color.green);
                Debug.DrawLine(p1, p2, Color.green);
                Debug.DrawLine(p2, p3, Color.green);
                Debug.DrawLine(p3, p0, Color.green);
            }
#endif
        }

        public void OnDestroy(ref SystemState state)
        {
            if (Grid.IsCreated)
                Grid.Dispose();
        }


        [BurstCompile]
        public partial struct PlayerCircleSpatialGridJob : IJobEntity
        {
            public NativeParallelMultiHashMap<int2, Entity> Grid;

            void Execute(Entity entity, in LocalTransform transform, in PlayerTag tag,
                ref DynamicBuffer<CircleBuffer> buffers)
            {
                foreach (var buffer in buffers)
                {
                    float radius = buffer.Radius;
                    float2 position = transform.Position.xy + buffer.Offset.xy;
                    float2 min = position - radius;
                    float2 max = position + radius;
                    int2 minCell = (int2)math.floor(min);
                    int2 maxCell = (int2)math.floor(max);
                    for (int x = minCell.x; x <= maxCell.x; x++)
                    {
                        for (int y = minCell.y; y <= maxCell.y; y++)
                        {
                            Grid.Add(new int2(x, y), entity);
                        }
                    }
                }
            }
        }

        [BurstCompile]
        public partial struct MonsterCircleSpatialGridJob : IJobEntity
        {
            public NativeParallelMultiHashMap<int2, Entity>.ParallelWriter Grid;

            void Execute(Entity entity, in LocalTransform transform, in AgentShape shape, in MonsterTag tag)
            {
                float radius = shape.Radius;
                float2 pos = transform.Position.xy;
                float2 min = pos - radius;
                float2 max = pos + radius;
                int2 minCell = (int2)math.floor(min);
                int2 maxCell = (int2)math.floor(max);
                for (int x = minCell.x; x <= maxCell.x; x++)
                {
                    for (int y = minCell.y; y <= maxCell.y; y++)
                    {
                        Grid.Add(new int2(x, y), entity);
                    }
                }
            }
        }
    }
}