using System;
using System.Collections.Generic;
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
    
        private Dictionary<int, StatData> byId;
    
        public override void OnMapValue()
        {
            byId = new Dictionary<int, StatData>();

            foreach (var statData in stats)
            {
                byId.Add(statData.ID, statData);
            }
        }

        public bool TryGetStatById(int statID, out StatData statData)
        {
            return byId.TryGetValue(statID, out statData);
        }
    }

    [Serializable]
    public struct StatData
    {
        public int ID;
        public string Name;
        public int Value;
        public float Modifier;
    }
}