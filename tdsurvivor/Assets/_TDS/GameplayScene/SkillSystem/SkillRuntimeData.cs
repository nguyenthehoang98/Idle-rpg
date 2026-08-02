using _Toolkit.SkillSystem.Core;
using UnityEngine;

namespace _TDS.GameplayScene.SkillSystem
{
    /// <summary>
    /// Runtime data khi cast skill — nạp từ Hero stats + buff, truyền vào SkillManager.
    /// </summary>
    public struct SkillRuntimeData
    {
        public int TargetEntity;
        public int SkillId;
        public string AssetName;
        public ProjectileRuntimeData Projectile;
        public float AttackRange;
        public int Attack;
        public float CritRate;
        public float CritDamage;
        public float CollisionDelayInit;
        public float CollisionDuration;
        public int HitCount;
        public float HitInterval;
        public int SpreadProjectileCount;
        public float SpreadAngleStep;
        public float SpreadDamageScale;
        public int ParallelProjectileCount;
        public float ParallelDistanceStep;
        public float ParallelDamageScale;
        public string ExplosiveAssetName;
        public float ExplosiveRadius;
        public float ExplosiveDamageScale;
        public float InstantKillTargetBelowHealthPercent;
        public Vector3 CastPivot;
        public Vector3 CastMuzzle;
        public Vector3 CastDestination;
    }
}