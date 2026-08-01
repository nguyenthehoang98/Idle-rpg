using System;
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
}