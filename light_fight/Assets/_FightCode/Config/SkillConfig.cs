using System;
using System.Collections.Generic;
using DG.DemiEditor.DeGUINodeSystem;
using ExcelExtension;
using UnityEngine;
using UnityEngine.Serialization;

namespace _FightCode.Config
{
    [ExcelAsset(
        ExcelPath = "Assets/Excels/SkillConfig.xlsx",
        ConfigPath = "Assets/_FightSource/Configs/SkillConfig.asset")]
    public class SkillConfig : BaseConfig
    {
        [SerializeField] private List<SkillData> skills = new List<SkillData>();

        private Dictionary<int, SkillData> byId;
    
        public override void OnMapValue()
        {
            byId = new Dictionary<int, SkillData>();

            foreach (var statData in skills)
            {
                byId.Add(statData.ID, statData);
            }
        }

        public bool TryGetSkillById(int skillID, out SkillData skillData)
        {
            return byId.TryGetValue(skillID, out skillData);
        }
    }

    [Serializable]
    public struct SkillData
    {
        public int ID;
        public string NameKey;
        public FindTargetType FindTarget;
        public float LifeTime;
        public SkillTriggerType Trigger;
        public float SkillTimerTrigger;
        public int SkillEventIdTrigger;
        public DamageTickerType DamageTicker;
        public int IsHealthPercent;
        public int MaximumCollision;
        public float CollisionResetInterval;
        public string Prefab;
        public float[] OffsetStartPosition;
        public ShapeType Shape;
        public float ShapeTimerTrigger;
        public float[] OffsetRelativePosition;
        public float[] SquareSize;
        public float CircleRadius;
        public float CapsuleHeight;
        public float CapsuleRadius;
        public TrajectoryType Trajectory;
        public float BulletInitialSpeed;
        public float BulletAcceleration;
        public string BoomerangCastingPhase;
        public string BoomerangReturningPhase;
        public string BlendPhase;
        public string ParabolicHeightPhase;
        public string ParabolicDistancePhase;
        public int SkillIDCastingWhenFinished;
    }

    public enum TrajectoryType
    {
        Stationary,
        Bullet,
        Boomerang,
        Blend,
        Parabolic
    }

    public enum ShapeType
    {
        Circle, Square, Capsule
    }

    public enum DamageTickerType
    {
        Instant, DamageOverTime
    }

    public enum SkillTriggerType
    {
        Timeline, EventId,
    }

    public enum FindTargetType
    {
        Random,
        Nearest,
        Farthest,
        HealthLowest,
        HealthHighest,
        DamageLowest,
        DamageHighest,
    }
}