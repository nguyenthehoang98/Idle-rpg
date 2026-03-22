using _Games.Combat.EntityComponentSystem.Data;
using Unity.Burst;
using Unity.Entities;

namespace _Games.Combat.EntityComponentSystem.System
{
    [BurstCompile]
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(ProjectileDamageMonsterSystem))]
    public partial struct ProjectileCleanupSystem : ISystem
    {
        ComponentLookup<ProjectileDeadTag> ProjectileDeadTagEvents;
        EntityQuery queryJob1;
        EntityQuery queryJob2;
      
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
            ProjectileDeadTagEvents = state.GetComponentLookup<ProjectileDeadTag>(true);
            
            queryJob1 = SystemAPI.QueryBuilder()
                .WithAll<ProjectileSkillData>()
                .WithNone<ProjectileDeadTag, ProjectileExplosion>()
                .Build();
            queryJob2 = SystemAPI.QueryBuilder()
                .WithAll<ProjectileSkillData>()
                .WithNone<ProjectileDeadTag>()
                .Build();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            ProjectileDeadTagEvents.Update(ref state);
            
            var ecb = SystemAPI
                .GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged)
                .AsParallelWriter();
            
            state.Dependency = new CollisionLimitedJob
            {
                ECB = ecb,
            }.ScheduleParallel(queryJob1, state.Dependency);
            state.Dependency = new EndCycleTimeJob
            { 
                ECB = ecb,
                DeltaTime = state.WorldUnmanaged.Time.DeltaTime,
            }.ScheduleParallel(queryJob2, state.Dependency);
            state.Dependency = new ResetHitBufferJob
            {
            }.ScheduleParallel(state.Dependency);
        }
        
        [BurstCompile]
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

        [BurstCompile]
        partial struct CollisionLimitedJob : IJobEntity
        {
            public EntityCommandBuffer.ParallelWriter ECB;

            public void Execute([ChunkIndexInQuery] int chunkIndex, Entity entity, in ProjectileSkillData skillData)
            {
                if (skillData.TotalUnitBeHit >= skillData.MaximumHits)
                {
                    ECB.AddComponent(chunkIndex, entity,
                        new ProjectileDeadTag(ProjectileDeadReason.Hit)
                    );
                }
            }
        }

        [BurstCompile]
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
                        new ProjectileDeadTag(ProjectileDeadReason.EndCycle)
                    );
                }
            }
        }
    }
}