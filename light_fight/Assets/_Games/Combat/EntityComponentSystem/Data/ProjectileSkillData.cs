using Unity.Entities;

namespace _Games.Combat.EntityComponentSystem.Data
{
    public struct ProjectileSkillData : IComponentData
    {
        public readonly Entity Parent;
        public readonly int SkillId;
        public readonly float CastTime;
        public readonly float LifeTime;
        public readonly int MaximumHits;
        public readonly float CollisionResetInterval;
        public readonly float FlatDamage;
        public readonly float ScaleDamage;

        public int TotalUnitBeHit;
        public double ElapsedLifeTime;
        public double ElapsedCollisionResetTime;
        
        public ProjectileSkillData(Entity parent, int skillId, float lifeTime, float castTime,
            int maximumHits, float collisionResetInterval, float flatDamage, float scaleDamage)
        {
            Parent = parent;
            SkillId = skillId;
            LifeTime = lifeTime;
            CastTime = castTime;
            MaximumHits = maximumHits;
            CollisionResetInterval = collisionResetInterval;
            FlatDamage = flatDamage;
            ScaleDamage = scaleDamage;
            TotalUnitBeHit = 0;
            ElapsedLifeTime = ElapsedCollisionResetTime = 0;
        }
    }
}