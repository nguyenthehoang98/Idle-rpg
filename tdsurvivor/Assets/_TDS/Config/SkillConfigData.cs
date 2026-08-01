using System;
using UnityEngine;

namespace _TDS.Config
{
    public enum TrajectoryType
    {
        
    }

    public enum FindTargetType
    {
    }
    
    [Serializable]
    public struct SkillConfigData
    {
        public int skillId;
        public string prefabName; // asset key của projectile prefab

        // Damage tick (DoT)
        public float damageTickInterval;

        // Query entity
        public FindTargetType findTarget;
        public float attackRange;

        siawr lai chio so phan nay
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
