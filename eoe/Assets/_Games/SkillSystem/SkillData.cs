using System;
using _KITSystem.SkillSystem.Core;

namespace _Games.SkillSystem
{
    [Serializable]
    public struct SkillData
    {
        public float LifeTime;
        public float DelayInitProjectileTime;
        public string Projectile;
        public FindTargetData FindTarget;
        public TrajectoryData Trajectory;
    }

    [Serializable]
    public struct TrajectoryData
    {
        public TrajectoryType Type;
        public float BulletInitialSpeed;
        public float BulletAcceleration;
    }

    [Serializable]
    public struct FindTargetData
    {
        public FindTargetType Type;
        public FilterParameter Parameter;
        public float Radius;

        public enum FilterParameter
        {
            None = 0,
            HpLowest,
            HpHighest,
            AtkLowest,
            AtkHighest,
            DefLowest,
            DefHighest
        }
    }

    public enum TrajectoryType
    {
        Bullet = 1,
    }
}