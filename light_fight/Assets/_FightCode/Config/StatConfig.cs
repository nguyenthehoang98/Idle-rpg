using System;
using System.Collections.Generic;
using _KITSystem.Formula;
using ExcelExtension;
using UnityEngine;

namespace _FightCode.Config
{
    [ExcelAsset(
        ExcelPath = "Assets/Excels/StatConfig.xlsx",
        ConfigPath = "Assets/_FightSource/Configs/StatConfig.asset")]
    public class StatConfig : BaseConfig
    {
        [SerializeField] private List<StatData> stats = new List<StatData>();
    
        private Dictionary<StatType, StatData> cacheStatData;
    
        public override void OnMapValue()
        {
            cacheStatData = new Dictionary<StatType, StatData>();

            foreach (var m in stats)
            {
                cacheStatData.Add(m.type, m);
            }
        }

        public bool Find(StatType stat, out StatData statData)
        {
            return cacheStatData.TryGetValue(stat, out statData);
        }
    }

    [Serializable]
    public struct StatData
    {
        public int id;
        public StatType type;
        public float value;
        public int modifier;
    }
}