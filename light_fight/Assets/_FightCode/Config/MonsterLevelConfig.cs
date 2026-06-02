using System;
using System.Collections.Generic;
using ExcelExtension;
using UnityEngine;

namespace _FightCode.Config
{
    [ExcelAsset(
        ExcelPath = "Assets/Excels/MonsterLevelConfig.xlsx",
        ConfigPath = "Assets/_FightSource/Configs/MonsterLevelConfig.asset")]
    public class MonsterLevelConfig : BaseConfig
    {
        [SerializeField] private List<MonsterLevelData> levels = new List<MonsterLevelData>();
    
        private Dictionary<int, MonsterLevelData> byLevel;
    
        public override void OnMapValue()
        {
            byLevel = new Dictionary<int, MonsterLevelData>();

            foreach (var data in levels)
            {
                byLevel.Add(data.Level, data);
            }
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