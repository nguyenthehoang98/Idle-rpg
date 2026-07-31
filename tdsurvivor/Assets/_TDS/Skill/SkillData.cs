using System;
using _GameToolkit.SkillSystem.Core;
using _GameToolkit.SkillSystem.Imp;
using UnityEngine;

namespace _TDS.Skill
{
    /// <summary>
    /// Runtime data khi cast skill — nạp từ Hero stats + buff, truyền vào SkillManager.
    /// </summary>
    [Serializable]
    public struct SkillRuntimeData
    {
        public int Entity;
        public float ProjectileScaleBonus;
        public float AttackRange;
        public float Attack;
        public float CritChance;
        public float CritDamage;
        public int ProjectilesPerShot;
        public int SpreadProjectileCount;
        public int BonusPierceCount;
        public float ExplosiveRadius;
        public float ExplosiveDamagePercent;
        public string ExplosivePrefabName;
        public float ExecuteHealthPercent;
        public TrajectoryData Trajectory;
        public Vector2 Pivot;
        public Vector2 Muzzle;
        public Vector2 Destination;
    }

    [Serializable]
    public struct SkillData
    {
        public int skillId;
        public string prefabName; // asset key của projectile prefab

        // Damage tick (DoT)
        public float damageTickInterval;

        // Query entity
        public FindTargetType findTarget;
        public float attackRange;

        // Trajectory
        public TrajectoryType trajectory;
        public float projectileDuration;
        public float boomerangOutboundDuration;
        public float boomerangReturnDuration;
        public float boomerangHangDuration;
        public TrajectoryStationaryPivot stationaryPivot;
        public float stationaryRandomRadius;
        public float stationaryDuration;
        public float splineWindupDuration;
        public float splineExecuteDuration;
        public float splineRecoveryDuration;

        // Collider
        public float collisionStartDelay;
        public float collisionDuration;
        public float targetHitCooldown;
        public int maxHitCount;

        // Extra/bonus
        public float spreadAngleStep;
        public float parallelDistanceStep;
    }

    public enum TrajectoryStationaryPivot
    {
        Weapon, Enemy, Random
    }
}
