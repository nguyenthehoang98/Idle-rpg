using System;
using System.Collections.Generic;
using _Toolkit.Config;
using ExcelExtension;
using UnityEngine;

namespace _TDS.Config
{
    [Serializable, ExcelAsset(
         ExcelPath = "Assets/Excels/MonsterConfig.xlsx",
         ConfigPath = "Assets/_TDS assets/Config/MonsterConfig.json")]
    public class MonsterConfig : IConfig
    {
        [SerializeField] private List<MonsterConfigData> monsters = new List<MonsterConfigData>();

        private Dictionary<int, MonsterConfigData> cachedMonsters;
        
        public void OnMappingValue()
        {
            cachedMonsters = new Dictionary<int, MonsterConfigData>();

            foreach (MonsterConfigData m in monsters)
            {
                if(!cachedMonsters.TryAdd(m.id, m)) Debug.LogError($"MonsterConfig couldn't be added to heroes '{m.id}'");
            }
        }

        public void OnImported()
        {
        }

        public void OnCompleteImported()
        {
        }

        public bool TryGetMonster(int id, out MonsterConfigData config)
        {
            return cachedMonsters.TryGetValue(id, out config);
        }
    }
}