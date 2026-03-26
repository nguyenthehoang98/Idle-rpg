using Unity.Entities;

namespace _Games.Combat.EntityComponentSystem.Data
{
    public struct ProjectileSkillData : IComponentData
    {
        public readonly Entity Parent;
        public readonly int SkillId;
        public readonly float LifeTime;
        public readonly int MaximumHits;
        public readonly float CollisionResetInterval;
        public readonly float FlatDamage;
        public readonly float ScaleDamage;

        public int TotalUnitBeHit;
        public double ElapsedLifeTime;
        public double ElapsedCollisionResetTime;

        public bool IsSourceMonster() => Parent != Entity.Null;
        public bool IsSourcePlayer() => Parent == Entity.Null;
        
        public ProjectileSkillData(Entity parent, int skillId, float lifeTime,
            int maximumHits, float collisionResetInterval, float flatDamage, float scaleDamage)
        {
            Parent = parent;
            SkillId = skillId;
            LifeTime = lifeTime;
            MaximumHits = maximumHits;
            CollisionResetInterval = collisionResetInterval;
            FlatDamage = flatDamage;
            ScaleDamage = scaleDamage;
            TotalUnitBeHit = 0;
            ElapsedLifeTime = ElapsedCollisionResetTime = 0;
        }
    }
}