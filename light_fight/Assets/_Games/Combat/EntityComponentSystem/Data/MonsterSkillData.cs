using Unity.Entities;

namespace _Games.Combat.EntityComponentSystem.Data
{
    public struct MonsterSkillData : IComponentData
    {
        public readonly int SkillId;
        public readonly int SkillLevel;
        public readonly float SkillCooldown;
        public readonly int MonsterId;
        public readonly float AttackRangeSq;
        public readonly float DelayCastTime;
        
        public bool IsLastTriggerSkill;
        public bool IsRunning;
        public double LastAttackTime;

        public MonsterSkillData(int monsterId, int skillId, int skillLevel, float skillCooldown,
            float attackRange, float delayCastTime)
        {
            MonsterId = monsterId;
            SkillId = skillId;
            SkillLevel = skillLevel;
            SkillCooldown = skillCooldown;
            AttackRangeSq = attackRange * attackRange;
            DelayCastTime = delayCastTime;
            IsLastTriggerSkill = false;
            IsRunning = true;
            LastAttackTime = 0;
        }

        public bool IsReady(double time)
        {
            return time >= LastAttackTime + SkillCooldown;
        }
    }
}