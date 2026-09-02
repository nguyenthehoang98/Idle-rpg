using System;
using _GameToolkit.Skills;

namespace _TDS.GameConfig
{
    [Serializable]
    public struct SkillConfigData
    {
        public int skillId;
        public string prefabName;
        public float damageTickInterval;
        public TargetSelectionType findTarget;
        public float attackRange;
        public float projectileSpeed;
        public float projectileStartDuration;
        public float projectileDuration;
        public float projectileEndDuration;
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
    }
}