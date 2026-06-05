using System;
using System.Collections.Generic;
using _KITSystem.Config;
using Newtonsoft.Json;
using UnityEngine;

namespace _FightCode.Config
{
    [Serializable]
    public class MonsterConfig : IGameConfig
    {
        [SerializeField] private List<MonsterData> Overview = new List<MonsterData>();
        [SerializeField] private List<MonsterClassData> Class_1 = new List<MonsterClassData>();
    
        public void OnMappingValue()
        {
        }

        public void OnPostImported()
        {
            for (var i = 0; i < Overview.Count; i++)
            {
                var data = Overview[i];
                data.OnImported();
                Overview[i] = data;
            }
            for (var i = 0; i < Class_1.Count; i++)
            {
                var data = Class_1[i];
                data.OnImported();
                Class_1[i] = data;
            }
        }
    }

    [Serializable]
    public struct MonsterData
    {
        public int ID;
        public string NameKey;
        public string Prefab;
        public int ClassID;
        public float StopDistance;
        
        [JsonProperty] private float AttackRangeMin;
        [JsonProperty] private float AttackRangeMax;
        
        public int ActiveSkillID; // kĩ năng active
        public int PassiveSkillID; // kĩ năng passive
        public float Scale;
        public float ColliderRadius;
        
        [JsonProperty] private float ColliderOffsetX;
        [JsonProperty] private float ColliderOffsetY;
        [JsonProperty] private float HealthBarOffsetX;
        [JsonProperty] private float HealthBarOffsetY;
        [JsonProperty] private float FloatingTextOffsetX;
        [JsonProperty] private float FloatingTextOffsetY;
        
        [JsonIgnore] public Vector2 ColliderOffset;
        [JsonIgnore] public Vector2 HealthBarOffset;
        [JsonIgnore] public Vector2 FloatingTextOffset;
        [JsonIgnore] public Vector2 AttackRange;
        
        public float KnockbackResistance;
        public float KnockbackResistanceCD;
        public float StunResistance;
        public float StunResistanceCD;
        public string DeathSfx;

        public void OnImported()
        {
            AttackRange = new Vector2(AttackRangeMin, AttackRangeMax);
            ColliderOffset = new Vector2(ColliderOffset.x, ColliderOffset.y);
            HealthBarOffset = new Vector2(HealthBarOffset.x, HealthBarOffset.y);
            FloatingTextOffset = new Vector2(FloatingTextOffset.x, FloatingTextOffset.y);
        }
    }

    [Serializable]
    public struct MonsterClassData
    {
        public int ID;
        public string Name;
        
        [JsonProperty] private float Health_A;
        [JsonProperty] private float Health_B;
        [JsonProperty] private float Attack_A;
        [JsonProperty] private float Attack_B;
        [JsonProperty] private float AttackSpeed_A;
        [JsonProperty] private float AttackSpeed_B;
        [JsonProperty] private float Speed_A;
        [JsonProperty] private float Speed_B;
        
        [JsonIgnore] public PowerFormula Health;
        [JsonIgnore] public PowerFormula Attack;
        [JsonIgnore] public LinearFormula AttackSpeed;
        [JsonIgnore] public LinearFormula Speed;
        
        public void OnImported()
        {
            Attack = new PowerFormula(1, Attack_A, Attack_B);
            Health = new PowerFormula(1, Health_A, Health_B);
            AttackSpeed = new LinearFormula(1, AttackSpeed_A, AttackSpeed_B);
            Speed = new LinearFormula(1, Speed_A, Speed_B);
        }
    }
}