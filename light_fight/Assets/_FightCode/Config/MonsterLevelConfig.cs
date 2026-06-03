using System;
using System.Collections.Generic;
using _KITSystem.Data;
using UnityEngine;

namespace _FightCode.Config
{
    [Serializable]
    public class MonsterLevelConfig : IGameConfig
    {
        [SerializeField] private List<MonsterLevelData> levels = new List<MonsterLevelData>();
    
        private Dictionary<int, MonsterLevelData> byLevel;
    
        public void OnMappingValue()
        {
            byLevel = new Dictionary<int, MonsterLevelData>();

            foreach (var data in levels)
            {
                byLevel.Add(data.Level, data);
            }
        }
        
        public void OnPostImported()
        {
        }

        public bool TryGetMonsterDataByLevel(int level, out MonsterLevelData monsterLevelData)
        {
            return byLevel.TryGetValue(level, out monsterLevelData);
        }
    }

    [Serializable]
    public struct MonsterLevelData
    {
        public int Level;
        public int Health;
        public int Attack;
        public int Cooldown;
    }
}