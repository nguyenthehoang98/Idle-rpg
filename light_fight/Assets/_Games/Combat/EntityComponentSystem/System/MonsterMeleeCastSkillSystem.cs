using _Games.Combat.EntityComponentSystem.Data;
using _Games.Combat.Model;
using ProjectDawn.Navigation;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace _Games.Combat.EntityComponentSystem.System
{
    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    public partial struct MonsterMeleeCastSkillSystem : ISystem
    {
        const float THRESHOLD = 1f;
        
        public void OnUpdate(ref SystemState state)
        {
            state.Dependency = new CastSkillJob
            {
                ElapsedTime = state.WorldUnmanaged.Time.ElapsedTime,
            }.ScheduleParallel(state.Dependency);
            state.Dependency.Complete();

            foreach ((RefRW<MonsterSkillData> skillData, RefRO<AgentBody> bod, Entity entity) in SystemAPI
                         .Query<RefRW<MonsterSkillData>, RefRO<AgentBody>>()
                         .WithAll<MonsterMeleeTag>()
                         .WithNone<MonsterDeadTag, MonsterBlockCastSkillTag>()
                         .WithEntityAccess())
            {
                MonsterSkillData data = skillData.ValueRO;
                if (data.AnimationName == AnimationName.Move && math.lengthsq(bod.ValueRO.Velocity) < THRESHOLD)
                {
                    data.AnimationName = AnimationName.Idle;
                    
                    if (!SystemAPI.ManagedAPI.HasComponent<Transform>(entity))
                        return;
                    Transform transform = SystemAPI.ManagedAPI.GetComponent<Transform>(entity);
                    Monster monster = transform.GetComponent<Monster>();
                    if (monster == null)
                        return;
                    monster.PlayAnimation(AnimationName.Idle);
                }
                
                if (data.AnimationName == AnimationName.Idle && math.lengthsq(bod.ValueRO.Velocity) > THRESHOLD)
                {
                    data.AnimationName = AnimationName.Move;
                    
                    if (!SystemAPI.ManagedAPI.HasComponent<Transform>(entity))
                        return;
                    Transform transform = SystemAPI.ManagedAPI.GetComponent<Transform>(entity);
                    Monster monster = transform.GetComponent<Monster>();
                    if (monster == null)
                        return;
                    monster.PlayAnimation(AnimationName.Move);
                }
                
                if (data.IsLastTriggerSkill)
                {
                    data.IsLastTriggerSkill = false;
                    
                    if (!SystemAPI.ManagedAPI.HasComponent<Transform>(entity))
                        return;
                    Transform transform = SystemAPI.ManagedAPI.GetComponent<Transform>(entity);
                    Monster monster = transform.GetComponent<Monster>();
                    if (monster == null)
                        return;
                    monster.QueueAnimation(AnimationName.Attack, AnimationName.Idle);
                }

                skillData.ValueRW = data;
            }
        }

        [BurstCompile]
        [WithNone(typeof(MonsterDeadTag), typeof(MonsterBlockCastSkillTag))]
        partial struct CastSkillJob : IJobEntity
        {
            public double ElapsedTime;

            public void Execute(in Entity entity, in MonsterMeleeTag tag, in LocalTransform transform, in AgentBody body,
                ref MonsterSkillData skill)
            {
                float3 position = transform.Position;
                float3 destination = float3.zero;
                if (math.distancesq(position, destination) <= skill.AttackRangeSq)
                {
                    if (skill.IsRunning && math.lengthsq(body.Velocity) < THRESHOLD)
                    {
                        skill.IsRunning = false;
                        skill.LastAttackTime = ElapsedTime - skill.SkillCooldown + skill.DelayCastTime;
                    }
                }
                else
                {
                    skill.LastAttackTime = ElapsedTime;
                    skill.IsRunning = true;
                }

                // todo: tính toán cast skill
                if (skill.IsRunning || !skill.IsReady(ElapsedTime)) return;

                skill.LastAttackTime = ElapsedTime;
                skill.IsLastTriggerSkill = true;
            }
        }
    }
}