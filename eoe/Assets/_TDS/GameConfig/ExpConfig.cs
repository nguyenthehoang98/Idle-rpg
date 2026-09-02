using System;
using System.Collections.Generic;
using _GameToolkit.GameConfig;
using ExcelExtension;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Serialization;

namespace _TDS.GameConfig
{
    [Serializable, ExcelAsset(
         ExcelPath = "Assets/Excels/ExpConfig.xlsx",
         ConfigPath = "Assets/_TDSAssets/Config/ExpConfig.json")]
    public class ExpConfig : Config
    {
        [SerializeField, JsonProperty] private List<ExpConfigData> exp = new List<ExpConfigData>();

        private Dictionary<int, ExpConfigData> cached;

        public override void OnMappingValue()
        {
            cached = new Dictionary<int, ExpConfigData>();

            foreach (var data in exp)
            {
                if (!cached.TryAdd(data.level, data)) Debug.LogError($"Duplicate exp level '{data.level}'");
            }
        }

        public bool TryGetExp(int level, out ExpConfigData data)
        {
            return cached.TryGetValue(level, out data);
        }
    }
}