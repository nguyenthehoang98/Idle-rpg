using System;
using _KITSystem.SkillSystem.Core;
using UnityEngine;

namespace _Game.GamePlay.SkillSystem
{
    [Serializable]
    public struct SkillData
    {
        public float LifeTime;
        public string Projectile;
        public FindTargetData FindTarget;
        public TrajectoryData Trajectory;
        public DamageTickerData DamageTicker;
        public ColliderData Collider;
    }

    [Serializable]
    public struct ColliderData
    {
        public float TimerTrigger;
        public float Duration;
        public int LimitNumberCollisions;
        public float ResetCollisionInterval;
        public ColliderType Type;
        public float Radius;
        public Vector2 RelativePosition;
    }

    [Serializable]
    public struct TrajectoryData
    {
        public TrajectoryType Type;
        public float BulletInitialSpeed;
        public float BulletAcceleration;
    }

    [Serializable]
    public struct FindTargetData
    {
        public FindTargetType Type;
        public FilterParameter Parameter;
        public float Radius;

        public enum FilterParameter
        {
            None = 0,
            HpLowest,
            HpHighest,
            AtkLowest,
            AtkHighest,
            DefLowest,
            DefHighest
        }
    }

    [Serializable]
    public struct DamageTickerData
    {
        public DamageTickerType Type;
        public float DamageTickerInterval;
    }

    public enum TrajectoryType
    {
        Bullet = 1,
    }

    public enum ColliderType
    {
        Circle
    }
}