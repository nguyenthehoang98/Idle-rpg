using System;
using _TDS.Battle;

namespace _TDS.GameConfig
{
    public enum UpgradeCardKind
    {
        ShopItem,
        HeroStat,
        SkillStat,
    }

    public enum SkillUpgradeStat
    {
        ProjectileSpeed = 0,
        ProjectileDuration = 1,
        HitCount = 2,
        HitInterval = 3,
        CollisionDuration = 4,
        SizeMultiplier = 5,
        SpreadDamageScale = 6,
        ParallelDamageScale = 7,
        ExplosiveRadius = 8,
        ExplosiveDamageScale = 9,
        DamageTickInterval = 10,
        CollisionDelayInit = 11,
        ProjectileDistanceStep = 12,
        ProjectileAngleStep = 13,
        SpreadProjectileCount = 14,
        ParallelProjectileCount = 15,
        InstantKillTargetBelowHealthPercent = 16,
    }

    [Serializable]
    public struct UpgradeCardConfigData
    {
        public int id;
        public UpgradeCardKind kind;
        public string title;
        public string description;
        public int cost;
        public int heroId;
        public StatId heroStat;
        public float value;
        public bool percent;
        public int skillId;
        public SkillUpgradeStat skillStat;
    }
}
