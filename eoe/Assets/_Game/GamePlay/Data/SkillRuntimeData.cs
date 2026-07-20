using _Game.GamePlay.Model;
using _KITSystem.SkillSystem.Imp;
using UnityEngine;
using UnityEngine.Splines;

namespace _Game.GamePlay.Data
{
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
        public DependencyResetAttack DependencyReset; 
        public TrajectoryData Trajectory;
        public Vector2 Pivot;
        public Vector2 Muzzle;
        public Vector2 Destination;
        public bool UseWeapon;
        public BaseWeapon Weapon;
    }
}