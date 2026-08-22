using System;

namespace _TDS.Config
{
    [Serializable]
    public struct SkillConfigData
    {
        public int skillId;
        public string prefabName;
        public float damageTickInterval;
        public FindTargetType findTarget;
        public float attackRange;
        public float projectileSpeed;
        public float projectileStartDuration;
        public float projectileDuration;
        public float projectileEndDuration;
        public float projectileSize;
        public float collisionDelayInit;
        public float collisionDuration;
        public int hitCount;
        public float hitInterval;
        public float projectileDistanceStep;
        public float projectileAngleStep;
        public float spreadBonusProjectileCount;
        public float spreadDamageScale;
        public int parallelBonusProjectileCount;
        public float parallelDamageScale;
        public string explosiveAssetName;
        public float explosiveRadius;
        public float explosiveDamageScale;
        public float instantKillTargetBelowHealthPercent;
    }
}
