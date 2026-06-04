using System;
using System.Collections.Generic;
using _KITSystem.Data;
using UnityEngine;

namespace _FightCode.Config
{
    [Serializable]
    public class SkillConfig : IGameConfig
    {
        [SerializeField] private List<CurveData> Curve_7 = new List<CurveData>(); 
        [SerializeField] private List<TrajectoryData> Trajectory_6 = new List<TrajectoryData>(); 
        [SerializeField] private List<CollisionTicketData> CollisionTickets_5 = new List<CollisionTicketData>(); 
        [SerializeField] private List<ShapeData> Shapes_4 = new List<ShapeData>(); 
        [SerializeField] private List<DamageTicketData> DamageTickets_3 = new List<DamageTicketData>(); 
        [SerializeField] private List<FindTargetData> FindTargets_2 = new List<FindTargetData>(); 
        [SerializeField] private List<SkillTriggerData> SkillTriggers_1 = new List<SkillTriggerData>(); 
        [SerializeField] private List<SkillData> Overview = new List<SkillData>(); 
        
        public void OnMappingValue()
        {
            
        }

        public void OnPostImported()
        {
            for (var i = 0; i < Shapes_4.Count; i++)
            {
                var data = Shapes_4[i];
                data.OnImported();
                Shapes_4[i] = data;
            }
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
                {
                    if (float.TryParse(split[0], out float a) && float.TryParse(split[1], out float b))
                    {
                        OffsetRelativePosition = new Vector2(a, b);
                    }
                    else Debug.LogError("OffsetRelative is invalid " + ID);
                }
                else Debug.LogError("OffsetRelative is invalid " + ID);
            }

            if(!string.IsNullOrEmpty(SquareSize))
            {
                string[] split = SquareSize.Trim('[', ']').Split(',');
                if (split.Length == 2)
                {
                    if (float.TryParse(split[0], out float a) && float.TryParse(split[1], out float b))
                    {
                        Size = new Vector2(a, b);
                    }
                    else Debug.LogError("SquareSize is invalid " + ID);
                }
                else Debug.LogError("SquareSize is invalid " + ID);
            }
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
        public string Prefab;
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