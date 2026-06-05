using System;
using System.Collections.Generic;
using _KITSystem.Config;
using _KITSystem.SkillSystem.Model;
using Newtonsoft.Json;
using UnityEngine;

namespace _FightCode.Config
{
    [Serializable]
    public class SkillConfig : IGameConfig
    {
        [SerializeField] private List<TrajectoryData> Trajectory_6 = new List<TrajectoryData>(); 
        [SerializeField] private List<ShapeData> Shapes_4 = new List<ShapeData>(); 
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
    public struct TrajectoryData 
    {
        public int ID;
        public TrajectoryType Type;
        public float BulletInitialSpeed;
        public float BulletAcceleration;
        public string BoomerangCastingCurve;
        public string BoomerangReturningCurve;
        public string Curve;
        public string ParabolicHeightCurve;
        public string ParabolicDistanceCurve;
    }

    [Serializable]
    public struct ShapeData 
    {
        public int ID;
        public ShapeType ShapeType;
        public float TimerTrigger;
        
        [JsonProperty] private float OffsetRelativeX;
        [JsonProperty] private float OffsetRelativeY;
        [JsonProperty] private float SquareSizeX;
        [JsonProperty] private float SquareSizeY;
        
        public float CircleRadius;
        
        [JsonIgnore] public Vector2 OffsetRelative;
        [JsonIgnore] public Vector2 SquareSize;

        public void OnImported()
        {
            OffsetRelative = new Vector2(OffsetRelativeX, OffsetRelativeY);
            SquareSize = new Vector2(SquareSizeX, SquareSizeY);
        }
    }

    [Serializable]
    public struct SkillData 
    {
        public int ID;
        public string Name;
        public string Prefab;
        public float LifeTime;
        public FindTargetType FindTargetType;
        public float FindTargetRadius;
        public DamageTickerType DamageTicketType;
        public float DamageTicketInterval;
        public int MaxCollision;
        public float ResetCollisionInterval;
        public int ShapeID;
        public int TrajectoryID;
    }
}