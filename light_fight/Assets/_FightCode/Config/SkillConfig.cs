using System;
using System.Collections.Generic;
using ExcelExtension;
using UnityEngine;

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

#if UNITY_EDITOR
        public override void OnPostImported()
        {
            for (var i = 0; i < Curve_7.Count; i++)
            {
                var data = Curve_7[i];
                data.OnImported();
                Curve_7[i] = data;
            }

            for (var i = 0; i < Trajectory_6.Count; i++)
            {
                var data = Trajectory_6[i];
                data.OnImported();
                Trajectory_6[i] = data;
            }

            for (var i = 0; i < CollisionTickets_5.Count; i++)
            {
                var data = CollisionTickets_5[i];
                data.OnImported();
                CollisionTickets_5[i] = data;
            }

            for (var i = 0; i < Shapes_4.Count; i++)
            {
                var data = Shapes_4[i];
                data.OnImported();
                Shapes_4[i] = data;
            }
            
            for (var i = 0; i < DamageTickets_3.Count; i++)
            {
                var data = DamageTickets_3[i];
                data.OnImported();
                DamageTickets_3[i] = data;
            }
            
            for (var i = 0; i < FindTargets_2.Count; i++)
            {
                var data = FindTargets_2[i];
                data.OnImported();
                FindTargets_2[i] = data;
            }
            
            for (var i = 0; i < SkillTriggers_1.Count; i++)
            {
                var data = SkillTriggers_1[i];
                data.OnImported();
                SkillTriggers_1[i] = data;
            }
            
            for (var i = 0; i < Overview.Count; i++)
            {
                var data = Overview[i];
                data.OnImported();
                Overview[i] = data;
            }
        }
#endif
    }

    [Serializable]
    public struct CurveData : IKitData
    {
        public int ID;
        public string Path;
        
        public void OnImported()
        {
        }
    }

    [Serializable]
    public struct TrajectoryData : IKitData
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
        
        public void OnImported()
        {
        }
    }

    [Serializable]
    public struct CollisionTicketData : IKitData
    {
        public int ID;
        public int MaxCollision;
        public float ResetInterval;
        
        public void OnImported()
        {
        }
    }

    [Serializable]
    public struct ShapeData : IKitData
    {
        public int ID;
        public ShapeType ShapeType;
        public float TimerTrigger;
        [SerializeField, HideInInspector] private string OffsetRelative;
        [SerializeField, HideInInspector] private string SquareSize;
        public SquarePivotType SquarePivot;
        [SerializeField, HideInInspector] private float CircleRadius;
        public Vector2 OffsetRelativePosition;
        public Vector2 Size;
        public float Radius;

        public void OnImported()
        {
            Radius = CircleRadius;

            if(!string.IsNullOrEmpty(OffsetRelative))
            {
                string[] split = OffsetRelative.Trim('[', ']').Split(',');
                if (split.Length == 2)
                    OffsetRelativePosition = new Vector2(
                        float.Parse(split[0]),
                        float.Parse(split[1])
                    );
                else Debug.LogError("OffsetRelative is invalid " + ID);
            }

            if(!string.IsNullOrEmpty(SquareSize))
            {
                string[] split = SquareSize.Trim('[', ']').Split(',');
                if (split.Length == 2)
                    Size = new Vector2(
                        float.Parse(split[0]),
                        float.Parse(split[1])
                    );
                else Debug.LogError("SquareSize is invalid " + ID);
            }
        }
    }

    [Serializable]
    public struct DamageTicketData : IKitData
    {
        public int ID;
        public DamageTickerType Type;
        public bool IsHealthPercent;
        
        public void OnImported()
        {
        }
    }

    [Serializable]
    public struct FindTargetData : IKitData
    {
        public int ID;
        public FindTargetType Type;
        
        public void OnImported()
        {
        }
    }

    [Serializable]
    public struct SkillTriggerData : IKitData
    {
        public int ID;
        public SkillTriggerType Type;
        public float Timer;
        public int EventID;
        
        public void OnImported()
        {
        }
    }

    [Serializable]
    public struct SkillData : IKitData
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
        
        public void OnImported()
        {
        }
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