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
        [SerializeField] private List<CurveData> Curve_7 = new List<CurveData>(); 
        [SerializeField] private List<TrajectoryData> Trajectory_6 = new List<TrajectoryData>(); 
        [SerializeField] private List<CollisionTicketData> CollisionTickets_5 = new List<CollisionTicketData>(); 
        [SerializeField] private List<ShapeData> Shapes_4 = new List<ShapeData>(); 
        [SerializeField] private List<DamageTicketData> DamageTickets_3 = new List<DamageTicketData>(); 
        [SerializeField] private List<FindTargetData> FindTargets_2 = new List<FindTargetData>(); 
        [SerializeField] private List<SkillTriggerData> SkillTriggers_1 = new List<SkillTriggerData>(); 
        [SerializeField] private List<SkillData> Overview = new List<SkillData>(); 
        
        public override void OnMapValue()
        {
            
        }
    }

    [Serializable]
    public struct CurveData
    {
        public int ID;
        public string Path;
    }

    [Serializable]
    public struct TrajectoryData
    {
        public int ID;
        public TrajectoryType Type;
        public float BulletInitialSpeed;
        public float BulletAcceleration;
        public int BoomerangCastingCurveID;
        public int BoomerangReturningCurveID;
        public int BlendCurveID;
        public int ParabolicHeightCurveID;
        public int ParabolicDistanceCurveID;
    }

    [Serializable]
    public struct CollisionTicketData
    {
        public int ID;
        public int MaxCollision;
        public float ResetInterval;
    }

    [Serializable]
    public struct ShapeData
    {
        public int ID;
        public ShapeType ShapeType;
        public float TimerTrigger;
        [SerializeField] private float[] OffsetRelative;
        [SerializeField] private float[] SquareSize;
        public SquarePivotType SquarePivot;
        [SerializeField] private float CircleRadius;

        public Vector3 OffsetRelativePosition
        {
            get
            {
                if (OffsetRelative.Length == 2)
                {
                    return new Vector3(OffsetRelative[0], OffsetRelative[1]);
                }

                return Vector3.zero;
            }
        }

        public Vector2 Size
        {
            get
            {
                if (SquareSize.Length == 2)
                {
                    return new Vector2(SquareSize[0], SquareSize[1]);
                }

                return Vector2.zero;
            }
        }

        public float Radius
        {
            get => CircleRadius;
        }
    }

    [Serializable]
    public struct DamageTicketData
    {
        public int ID;
        public DamageTickerType Type;
        public bool IsHealthPercent;
    }

    [Serializable]
    public struct FindTargetData
    {
        public int ID;
        public FindTargetType Type;
    }

    [Serializable]
    public struct SkillTriggerData
    {
        public int ID;
        public SkillTriggerType Type;
        public float Timer;
        public int EventID;
    }

    [Serializable]
    public struct SkillData
    {
        public int ID;
        public string Name;
        public float LifeTime;
        public int TriggerID;
        public int DamageTicketID;
        public int ShapeID;
        public int CollisionTicketID;
        public int TrajectoryID;
        public int SkillIDTriggerOnComplete;
    }
    
    public enum TrajectoryType
    {
        Stationary, Bullet, Boomerang, Blend, Parabolic
    }

    public enum ShapeType
    {
        Circle, Square
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
        Random, Nearest, Farthest, HealthLowest, HealthHighest, DamageLowest, DamageHighest,
    }

    public enum SquarePivotType
    {
        Center, BottomLeft, BottomRight, TopLeft, TopRight,
    }
}