using _Games.Combat.EntityComponentSystem.Data;
using _Games.Combat.EntityComponentSystem.Model;
using _Games.Combat.Model;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace _Games.Combat.EntityComponentSystem.System
{
    [BurstCompile]
    [UpdateAfter(typeof(ProjectileDamageSystem))]
    [UpdateInGroup(typeof(BattleSimulationGroup))]
    [RequireMatchingQueriesForUpdate]
    public partial struct ProjectileCleanupSystem : ISystem
    {
        ComponentLookup<ProjectileDestroyTag> ProjectileDeadTagEvents;
        EntityQuery queryJob1;
        EntityQuery queryJob2;
      
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
            ProjectileDeadTagEvents = state.GetComponentLookup<ProjectileDestroyTag>(true);
            
            queryJob1 = SystemAPI.QueryBuilder()
                .WithAll<ProjectileSkillData>()
                .WithNone<ProjectileDestroyTag, ProjectileExplosion>()
                .Build();
            queryJob2 = SystemAPI.QueryBuilder()
                .WithAll<ProjectileSkillData>()
                .WithNone<ProjectileDestroyTag>()
                .Build();
        }

        public void OnUpdate(ref SystemState state)
        {
            ProjectileDeadTagEvents.Update(ref state);

            EntityCommandBuffer ecb = SystemAPI
                .GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);
            EntityCommandBuffer.ParallelWriter ecb2 = ecb.AsParallelWriter();
            state.Dependency = new CollisionLimitedJob
            {
                ECB = ecb2,
            }.ScheduleParallel(queryJob1, state.Dependency);
            state.Dependency = new EndCycleTimeJob
            {
                ECB = ecb2,
                DeltaTime = state.WorldUnmanaged.Time.DeltaTime,
            }.ScheduleParallel(queryJob2, state.Dependency);
            state.Dependency = new ResetHitBufferJob
            {
            }.ScheduleParallel(state.Dependency);
            state.Dependency.Complete();

            // todo: post event
            foreach (var (transform, skillData, dead, entity) in
                     SystemAPI.Query<RefRO<LocalTransform>, RefRO<ProjectileSkillData>, RefRW<ProjectileDestroyTag>>()
                         .WithAll<ProjectileTag>()
                         .WithEntityAccess())
            {
                ProjectileDestroyTag destroyRo = dead.ValueRO;
                if (!destroyRo.IsTrigger)
                {
                    destroyRo.IsTrigger = true;
                    dead.ValueRW = destroyRo;

                    if (destroyRo.Reason == ProjectileDeadReason.Hit)
                    {
                        CastHitEffect(skillData.ValueRO.SkillId, transform.ValueRO.Position);
                    }

                    Projectile projectile = state.EntityManager.GetComponentObject<Projectile>(entity);
                    projectile.Destroy();
                    ecb.DestroyEntity(entity);
                }
            }
        }

        async void CastHitEffect(int skillId, float3 position)
        {
            Debug.LogWarning($"CastHitEffect: skillId={skillId}, position={position}");
        }

        partial struct ResetHitBufferJob : IJobEntity
        {
            public void Execute(in Entity entity, ref DynamicBuffer<CollisionBuffer> buffer, ref ProjectileSkillData skillData)
            {
                if (skillData.ElapsedCollisionResetTime >= skillData.CollisionResetInterval)
                {
                    buffer.Clear();
                    skillData.ElapsedCollisionResetTime = 0;
                }
            }
        }

        partial struct CollisionLimitedJob : IJobEntity
        {
            public EntityCommandBuffer.ParallelWriter ECB;

            public void Execute([ChunkIndexInQuery] int chunkIndex, Entity entity, in ProjectileSkillData skillData)
            {
                if (skillData.TotalUnitBeHit >= skillData.MaximumHits)
                {
                    ECB.AddComponent(chunkIndex, entity,
                        new ProjectileDestroyTag(ProjectileDeadReason.Hit)
                    );
                }
            }
        }

        partial struct EndCycleTimeJob : IJobEntity
        {
            public EntityCommandBuffer.ParallelWriter ECB;
            public double DeltaTime;

            public void Execute([ChunkIndexInQuery] int chunkIndex, Entity entity, ref ProjectileSkillData skillData)
            {
                skillData.ElapsedLifeTime += DeltaTime;
                skillData.ElapsedCollisionResetTime += DeltaTime;

                if (skillData.ElapsedLifeTime >= skillData.LifeTime)
                {
                    ECB.AddComponent(chunkIndex, entity,
                        new ProjectileDestroyTag(ProjectileDeadReason.EndCycle)
                    );
                }
            }
        }
    }
}