using System;
using System.Collections.Generic;
using _GameToolkit.GameConfig;
using ExcelExtension;
using Newtonsoft.Json;
using UnityEngine;

namespace _TDS.GameConfig
{
    [Serializable, ExcelAsset(
         ExcelPath = "Assets/Excels/MonsterConfig.xlsx",
         ConfigPath = "Assets/_TDSAssets/Config/MonsterConfig.json")]
    public class MonsterConfig : IConfig
    {
        [SerializeField, JsonProperty] private List<MonsterConfigData> monsters = new List<MonsterConfigData>();

        private Dictionary<int, MonsterConfigData> cached;

        public void OnMappingValue()
        {
            cached = new Dictionary<int, MonsterConfigData>();

            foreach (var data in monsters)
            {
                if (!cached.TryAdd(data.id, data)) Debug.LogError($"Duplicate monster '{data.id}'");
            }
        }

        public void OnImported()
        {
        }

        public void OnCompleteImported()
        {
        }

        public bool TryGetMonster(int monsterId, out MonsterConfigData data)
        {
            return cached.TryGetValue(monsterId, out data);
        }
    }
}
