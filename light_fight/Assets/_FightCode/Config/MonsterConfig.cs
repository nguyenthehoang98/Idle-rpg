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
            for (var i = 0; i < Overview.Count; i++)
            {
                var data = Overview[i];
                data.OnImported();
                Overview[i] = data;
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
        [SerializeField, HideInInspector] private string AttackRange;
        public int ActiveSkillID;         // kĩ năng active
        public int PassiveSkillID;        // kĩ năng passive
        public float Scale;
        public float ColliderRadius;
        [SerializeField, HideInInspector] private string ColliderOffset;
        [SerializeField, HideInInspector] private string HealthBarOffset;
        [SerializeField, HideInInspector] private string FloatingTextOffset;
        public float KnockbackResistance;
        public float KnockbackResistanceCD;
        public float StunResistance;
        public float StunResistanceCD;
        public string DeathSfx;

        public Vector2 AttackRangeMinMax;
        public Vector2 CollisionOffsetLocalPosition;
        public Vector2 HealthBarOffsetLocalPosition;
        public Vector2 FloatingTextOffsetLocalPosition;

        public void OnImported()
        {
            if(!string.IsNullOrEmpty(AttackRange))
            {
                string[] split = AttackRange.Trim('[', ']').Split(',');
                if (split.Length == 2)
                    AttackRangeMinMax = new Vector2(
                        float.Parse(split[0]),
                        float.Parse(split[1])
                    );
                else Debug.LogError("AttackRange is invalid " + ID);
            }
            
            if(!string.IsNullOrEmpty(ColliderOffset))
            {
                string[] split = ColliderOffset.Trim('[', ']').Split(',');
                if (split.Length == 2)
                    CollisionOffsetLocalPosition = new Vector2(
                        float.Parse(split[0]),
                        float.Parse(split[1])
                    );
                else Debug.LogError("ColliderOffset is invalid " + ID);
            }
                
            if(!string.IsNullOrEmpty(HealthBarOffset))
            {
                string[] split = HealthBarOffset.Trim('[', ']').Split(',');
                if (split.Length == 2)
                    HealthBarOffsetLocalPosition = new Vector2(
                        float.Parse(split[0]),
                        float.Parse(split[1])
                    );
                else Debug.LogError("HealthBarOffset is invalid " + ID);
            }      
        
            if(!string.IsNullOrEmpty(FloatingTextOffset))
            {
                string[] split = FloatingTextOffset.Trim('[', ']').Split(',');
                if (split.Length == 2)
                    FloatingTextOffsetLocalPosition = new Vector2(
                        float.Parse(split[0]),
                        float.Parse(split[1])
                    );
                else Debug.LogError("FloatingTextOffset is invalid " + ID);
            }
        }
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