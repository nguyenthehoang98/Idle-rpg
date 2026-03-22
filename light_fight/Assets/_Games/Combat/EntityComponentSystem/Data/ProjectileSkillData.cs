using Unity.Entities;

namespace _Games.Combat.EntityComponentSystem.Data
{
    public struct ProjectileSkillData : IComponentData
    {
        public readonly Entity Parent;
        public readonly float CastTime;
        public readonly float LifeTime;
        public readonly int MaximumHits;
        public readonly float CollisionResetInterval;

        public int TotalUnitBeHit;
        public double ElapsedLifeTime;
        public double ElapsedCollisionResetTime;
        
        public ProjectileSkillData(Entity parent, float lifeTime, float castTime,
            int maximumHits, float collisionResetInterval)
        {
            Parent = parent;
            LifeTime = lifeTime;
            CastTime = castTime;
            MaximumHits = maximumHits;
            CollisionResetInterval = collisionResetInterval;
            TotalUnitBeHit = 0;
            ElapsedLifeTime = ElapsedCollisionResetTime = 0;
        }
    }
}