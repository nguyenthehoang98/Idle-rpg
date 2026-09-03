using System;
using System.Collections.Generic;
using _GameToolkit.GameConfig;
using ExcelExtension;
using Newtonsoft.Json;
using UnityEngine;

namespace _TDS.GameConfig
{
    [Serializable, ExcelAsset(
         ExcelPath = "Assets/Excels/HeroConfig.xlsx",
         ConfigPath = "Assets/_TDSAssets/Config/HeroConfig.json")]
    public class HeroConfig : Config
    {
        [SerializeField, JsonProperty] private List<HeroConfigData> heros = new List<HeroConfigData>();

        private Dictionary<int, HeroConfigData> cached;

        public override void OnMappingValue()
        {
            cached = new Dictionary<int, HeroConfigData>();

            foreach (var data in heros)
            {
                if (!cached.TryAdd(data.id, data)) Debug.LogError($"Duplicate hero '{data.id}'");
            }
        }

        public bool TryGetHero(int heroId, out HeroConfigData data)
        {
            return cached.TryGetValue(heroId, out data);
        }
    }
}