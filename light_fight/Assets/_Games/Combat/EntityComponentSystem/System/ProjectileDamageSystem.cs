using _Games.Combat.EntityComponentSystem.Data;
using _Games.Combat.Event;
using _Games.Combat.Model;
using _Games.Utils;
using _KIT.Event;
using ProjectDawn.Custom;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace _Games.Combat.EntityComponentSystem.System
{
    [BurstCompile]
    [UpdateAfter(typeof(ProjectileOverlapSystem))]
    [UpdateInGroup(typeof(CustomSimulationGroup))]
    [RequireMatchingQueriesForUpdate]
    public partial struct ProjectileDamageSystem : ISystem
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

            EntityCommandBuffer ecb = SystemAPI
                .GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);
            
            foreach (var (skillData, transform, projectile) in 
                     SystemAPI.Query<RefRW<ProjectileSkillData>, RefRO<LocalTransform>>()
                         .WithAll<ProjectileTag>()
                         .WithEntityAccess())
            {
                ProjectileSkillData skillDataRO = skillData.ValueRO;
                DynamicBuffer<CollisionBuffer> buffers = collisionBufferLookup[projectile];
                for (int i = 0; i < buffers.Length; i++)
                {
                    CollisionBuffer b = buffers[i];
                    Entity entity = b.Entity;
                    Entity parent = skillData.ValueRO.Parent;
                    
                    // todo: on dmg player
                    if (!b.OnTrigger && state.EntityManager.HasComponent<PlayerTag>(entity) && parent != Entity.Null)
                    {
                        DefaultStatData statData = state.EntityManager.GetComponentData<DefaultStatData>(parent);
                            
                        HealthData healthData = healthDataLookup[entity];
                        healthData.Health -= FormulaUtils.Output(statData.Attack, skillDataRO.FlatDamage,
                            skillDataRO.ScaleDamage, 0, 0, 0
                        );
                        healthDataLookup[entity] = healthData;
                        EventBus.Instance.Publish(new PlayerOnDamageEvent());

                        b.OnTrigger = true;
                        buffers[i] = b;
                    }
                    
                    //todo: on dmg monster
                    if (!b.OnTrigger && state.EntityManager.HasComponent<MonsterTag>(entity) && parent == Entity.Null)
                    {
                        int attack = 10;// Sau lấy chỉ số từ weapon -> tạo weapon entity
                        HealthData healthData = healthDataLookup[entity];
                        int damage = FormulaUtils.Output(attack, skillDataRO.FlatDamage,
                            skillDataRO.ScaleDamage, 0, 0, 0
                        );
                        healthData.Health -= damage;
                        healthDataLookup[entity] = healthData;

                        Monster monster = null;
                        if (state.EntityManager.HasComponent<MonsterRangedTag>(entity))
                        {
                            monster = state.EntityManager.GetComponentObject<RangedMonster>(entity);  
                            if(monster == null) Debug.LogError("ranged null");
                        }
                        else if (state.EntityManager.HasComponent<MonsterMeleeTag>(entity))
                        {
                            monster = state.EntityManager.GetComponentObject<Monster>(entity);
                            if(monster == null) Debug.LogError("melee null");
                        }

                        if (monster != null)
                        {
                            monster.TakeDamage(damage, transform.ValueRO.Position);

                            if (healthData.Health <= 0)
                            {
                                monster.Death();
                                
                                ecb.DestroyEntity(entity);
                            }
                        }
                        else Debug.LogError("Monster not found: " + entity.ToString());
                        
                        b.OnTrigger = true;
                        buffers[i] = b;
                    }
                }
            }
        }
    }
}