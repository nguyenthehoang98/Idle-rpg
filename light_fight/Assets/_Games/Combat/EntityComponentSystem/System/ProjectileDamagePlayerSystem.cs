using _Games.Combat.EntityComponentSystem.Data;
using _Games.Combat.Event;
using _Games.Utils;
using _KIT.Event;
using Unity.Burst;
using Unity.Entities;

namespace _Games.Combat.EntityComponentSystem.System
{
    [BurstCompile]
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(ProjectileOverlapSystem))]
    public partial struct ProjectileDamagePlayerSystem : ISystem
    {
        ComponentLookup<HealthData> healthDataLookup;
        BufferLookup<CollisionBuffer> collisionBufferLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
            healthDataLookup = state.GetComponentLookup<HealthData>();
            collisionBufferLookup = state.GetBufferLookup<CollisionBuffer>();
        }

        public void OnUpdate(ref SystemState state)
        {
            healthDataLookup.Update(ref state);
            collisionBufferLookup.Update(ref state);

            foreach (var (skillData, entity) in 
                     SystemAPI.Query<RefRW<ProjectileSkillData>>()
                         .WithAll<ProjectileTag>()
                         .WithEntityAccess())
            {
                ProjectileSkillData skillDataRO = skillData.ValueRO;
                DynamicBuffer<CollisionBuffer> buffers = collisionBufferLookup[entity];
                for (int i = 0; i < buffers.Length; i++)
                {
                    CollisionBuffer b = buffers[i];
                    if (!b.OnTrigger)
                    {
                        b.OnTrigger = true;
                        buffers[i] = b;
                        Entity unit = b.Entity;
                        if (state.EntityManager.HasComponent<PlayerTag>(unit))
                        {
                            DefaultStatData statData = state.EntityManager.GetComponentData<DefaultStatData>(skillData.ValueRO.Parent);
                            
                            HealthData healthData = healthDataLookup[unit];
                            healthData.Health -= FormulaUtils.Output(statData.Attack, skillDataRO.SkillBaseDamage,
                                skillDataRO.SkillScaleDamage, 0, 0, 0
                            );
                            healthDataLookup[unit] = healthData;
                            EventBus.Instance.Publish(new PlayerOnDamageEvent());
                        }
                    }
                }
            }
        }
    }
}