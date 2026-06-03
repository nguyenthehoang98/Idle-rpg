using System;
using System.Collections.Generic;
using _KITSystem.Data;
using UnityEngine;

namespace _FightCode.Config
{
    [Serializable]
    public class MonsterClassConfig : IGameConfig
    {
        [SerializeField] private List<MonsterClassData> classes = new List<MonsterClassData>();
    
        private Dictionary<int, MonsterClassData> byId;
    
        public void OnMappingValue()
        {
            byId = new Dictionary<int, MonsterClassData>();

            foreach (var data in classes)
            {
                byId.Add(data.ID, data);
            }
        }
        
        public void OnPostImported()
        {
        }

        public bool TryGetMonsterClassByID(int classID, out MonsterClassData monsterClassData)
        {
            return byId.TryGetValue(classID, out monsterClassData);
        }
    }


    [Serializable]
    public struct MonsterClassData
    {
        public int ID;
        public float HealthScale;
        public float AttackScale;
        public float CooldownScale;
        public float SpeedScale;
    }
}