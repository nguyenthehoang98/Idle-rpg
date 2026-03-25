using _Games.Combat.EntityComponentSystem.Data;
using _Games.Combat.Model;
using ProjectDawn.Navigation;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace _Games.Combat.EntityComponentSystem.System
{
    [RequireMatchingQueriesForUpdate]
    public partial struct MonsterMeleeCastSkillSystem : ISystem
    {
        const float THRESHOLD = 1f;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PlayerTag>();
        }

        public void OnUpdate(ref SystemState state)
        {
            state.Dependency = new CastSkillJob
            {
                ElapsedTime = state.WorldUnmanaged.Time.ElapsedTime,
            }.ScheduleParallel(state.Dependency);
            state.Dependency.Complete();
            
            foreach ((RefRW<MonsterSkillData> skillData, RefRO<AgentBody> body, Entity entity) in SystemAPI
                         .Query<RefRW<MonsterSkillData>, RefRO<AgentBody>>()
                         .WithAll<MonsterMeleeTag>()
                         .WithNone<MonsterDeadTag, MonsterBlockCastSkillTag>()
                         .WithEntityAccess())
            {
                MonsterSkillData data = skillData.ValueRO;
                if (data.AnimationName == AnimationName.Move && math.lengthsq(body.ValueRO.Velocity) < THRESHOLD)
                {
                    data.AnimationName = AnimationName.Idle;
                    Monster monster = state.EntityManager.GetComponentObject<Monster>(entity);
                    monster.PlayAnimation(AnimationName.Idle);
                }
                
                if (data.AnimationName == AnimationName.Idle && math.lengthsq(body.ValueRO.Velocity) > THRESHOLD)
                {
                    data.AnimationName = AnimationName.Move;
                    Monster monster = state.EntityManager.GetComponentObject<Monster>(entity);
                    monster.PlayAnimation(AnimationName.Move);
                }
                
                if (data.IsLastTriggerSkill)
                {
                    data.IsLastTriggerSkill = false;
                    Monster monster = state.EntityManager.GetComponentObject<Monster>(entity);
                    monster.Attack();
                    monster.PlayAttackAnimation();
                }

                skillData.ValueRW = data;
            }
        }

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