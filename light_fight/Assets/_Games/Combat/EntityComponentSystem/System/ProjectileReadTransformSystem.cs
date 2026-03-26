using _Games.Combat.EntityComponentSystem.Data;
using ProjectDawn.Custom;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Jobs;

namespace _Games.Combat.EntityComponentSystem.System
{
    [BurstCompile]
    [UpdateAfter(typeof(ProjectileTrajectorySystem))]
    [UpdateInGroup(typeof(CustomSimulationGroup))]
    [RequireMatchingQueriesForUpdate]
    public partial struct ProjectileReadTransformSystem : ISystem
    {
        EntityQuery m_Query;
        ComponentLookup<LocalTransform> m_TransformLookup;

        public void OnCreate(ref SystemState state)
        {
            m_Query = SystemAPI.QueryBuilder()
                .WithAll<ProjectileTrajectory>()
                .WithAllRW<Transform>()
                .WithAll<LocalTransform>()
                .Build();
            m_TransformLookup = state.GetComponentLookup<LocalTransform>(true);
        }

        public void OnDestroy(ref SystemState state) { }

        public void OnUpdate(ref SystemState state)
        {
            var entities = m_Query.ToEntityArray(Allocator.TempJob);
            var transformAcessArray = m_Query.GetTransformAccessArray();

            m_TransformLookup.Update(ref state);

            state.Dependency = new ReadAgentTransformJob
            {
                Entities = entities,
                TransformLookup = m_TransformLookup,
            }.Schedule(transformAcessArray, state.Dependency);
        }

        [BurstCompile]
        struct ReadAgentTransformJob : IJobParallelForTransform
        {
            [DeallocateOnJobCompletion] public NativeArray<Entity> Entities;

            [ReadOnly] public ComponentLookup<LocalTransform> TransformLookup;

            public void Execute(int index, TransformAccess transformAccess)
            {
                Entity entity = Entities[index];

                var transform = TransformLookup[entity];
                transformAccess.position = transform.Position;
                transformAccess.rotation = transform.Rotation;
            }
        }
    }
}