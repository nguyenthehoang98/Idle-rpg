using System;
using System.Collections.Generic;
using _KITSystem.Data;
using UnityEngine;

namespace _FightCode.Config
{
    [Serializable]
    public class MonsterConfig : IGameConfig
    {
        [SerializeField] private List<MonsterData> Overview = new List<MonsterData>();
        [SerializeField] private List<MonsterClassData> Class_1 = new List<MonsterClassData>();
        [SerializeField] private List<MonsterLevelData> Level_2 = new List<MonsterLevelData>();
    
        public void OnMappingValue()
        {
        }
        
        public void OnPostImported()
        {
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
        public float AttackRangeMin;
        public float AttackRangeMax;
        public int ActiveSkillID;         // kĩ năng active
        public int PassiveSkillID;        // kĩ năng passive
        public float Scale;
        public float ColliderRadius;
        public float ColliderOffsetX;
        public float ColliderOffsetY;
        public float HealthBarOffsetX;
        public float HealthBarOffsetY;
        public float FloatingTextOffsetX;
        public float FloatingTextOffsetY;
        public float KnockbackResistance;
        public float KnockbackResistanceCD;
        public float StunResistance;
        public float StunResistanceCD;
        public string DeathSfx;
    }
    
    [Serializable]
    public struct MonsterClassData
    {
        public int ID;
        public float HealthScale;
        public float AttackScale;
        public float AttackSpeedScale;
        public float SpeedScale;
    }

    [Serializable]
    public struct MonsterLevelData
    {
        public int Level;
        public int Health;
        public int Attack;
        public float AttackSpeed;
    }
}