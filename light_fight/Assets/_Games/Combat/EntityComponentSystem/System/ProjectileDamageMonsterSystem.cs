using _Games.Combat.EntityComponentSystem.Data;
using Unity.Burst;
using Unity.Entities;

namespace _Games.Combat.EntityComponentSystem.System
{
    [BurstCompile]
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(ProjectileOverlapSystem))]
    public partial struct ProjectileDamageMonsterSystem : ISystem
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
                .WithAll<ProjectileTrajectory>()
                .WithNone<ProjectileDeadTag, ProjectileExplosion>()
                .Build();
            queryJob2 = SystemAPI.QueryBuilder()
                .WithAll<ProjectileTrajectory>()
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
            public void Execute(in Entity entity, ref DynamicBuffer<ProjectileCollisionMonsterBuffer> buffer, ref ProjectileCombat combat)
            {
                if (combat.ElapsedResetTime >= combat.ResetBeHitMonsterInterval)
                {
                    buffer.Clear();
                    combat.ElapsedResetTime = 0;
                }
            }
        }

        [BurstCompile]
        partial struct CollisionLimitedJob : IJobEntity
        {
            public EntityCommandBuffer.ParallelWriter ECB;

            public void Execute([ChunkIndexInQuery] int chunkIndex, Entity entity, in ProjectileCombat combat)
            {
                if (combat.CountMonsterBeHit >= combat.MaximumHitMonster)
                {
                    ECB.AddComponent(chunkIndex, entity,
                        new ProjectileDeadTag(entity, ProjectileDeadTagReason.Hit)
                    );
                }
            }
        }

        [BurstCompile]
        partial struct EndCycleTimeJob : IJobEntity
        {
            public EntityCommandBuffer.ParallelWriter ECB;
            public double DeltaTime;

            public void Execute([ChunkIndexInQuery] int chunkIndex, Entity entity, ref ProjectileCombat combat)
            {
                combat.ElapsedLifeTime += DeltaTime;
                combat.ElapsedResetTime += DeltaTime;

                if (combat.ElapsedLifeTime >= combat.LifeTime)
                {
                    ECB.AddComponent(chunkIndex, entity,
                        new ProjectileDeadTag(entity, ProjectileDeadTagReason.EndCycle)
                    );
                }
            }
        }
    }
}