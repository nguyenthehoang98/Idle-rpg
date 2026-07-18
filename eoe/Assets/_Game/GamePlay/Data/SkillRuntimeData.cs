using _Game.GamePlay.Model;
using _KITSystem.SkillSystem.Imp;
using UnityEngine;
using UnityEngine.Splines;

namespace _Game.GamePlay.Data
{
    public struct SkillRuntimeData
    {
        public int Entity;
        public float Attack;
        public float CritChance;
        public float CritDamage;
        public int ParallelCount;
        public float ParallelDamagePercent;
        public int SpreadCount;
        public float SpreadDamagePercent;
        public int PiercingCount;
        public float ExplosiveRadius;
        public float ExplosiveDamagePercent;
        public string ExplosivePrefabName;
        public int BounceCount; // chưa có logic
        public float BounceDamagePercent; // chưa có logic
        public float KillInstantBelowHealthPercent;
        public TrajectoryData Trajectory;
        public Vector2 Pivot;
        public Vector2 Muzzle;
        public Vector2 Destination;
        public bool UseWeapon;
        public BaseWeapon Weapon;
    }
}