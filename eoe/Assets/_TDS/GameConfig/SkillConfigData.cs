using System;
using _GameToolkit.Skills;
using _TDS.Battle;

namespace _TDS.GameConfig
{
    public enum SkillModifierType
    {
        Slow = 1,
        Bleed = 2,
        Silence = 3,
        Stun = 4,
    }

    [Serializable]
    public struct SkillModifierData
    {
        public SkillModifierType type;
        public float weight;
        public float successRate;
        public float duration;
        public float value;
        public float tickInterval;
    }

    [Serializable]
    public struct StatUpgradeConfigData
    {
        public int id;
        public StatId stat;
        public float value;
        public bool percent;
    }

    [Serializable]
    public struct SkillConfigData
    {
        public int skillId;
        public string prefabName;
        public float damageTickInterval;
        public TargetSelectionType findTarget;
        public float projectileSpeed;
        public float projectileDuration;
        /// <summary>Offset local theo hướng bắn: x = tiến, y = ngang, z = depth.</summary>
        public UnityEngine.Vector3 projectileSpawnOffset;
        /// <summary>Độ cao control point của spline, random trong khoảng [min, max].</summary>
        public float arcHeightMin;
        public float arcHeightMax;
        /// <summary>true: spawn thẳng tại target (PlaceProjectile), không bay. false: bắn từ hero.</summary>
        public bool spawnAtTarget;
        public float sizeMultiplier;
        public float collisionDelayInit;
        public float collisionDuration;
        public int hitCount;
        public float hitInterval;
        public float projectileDistanceStep;
        public float projectileAngleStep;
        public int spreadProjectileCount;
        public float spreadDamageScale;
        public int parallelProjectileCount;
        public float parallelDamageScale;
        public string explosivePrefabName;
        public float explosiveRadius;
        public float explosiveDamageScale;
        public float instantKillTargetBelowHealthPercent;
        public SkillModifierData[] modifiers;
    }
}