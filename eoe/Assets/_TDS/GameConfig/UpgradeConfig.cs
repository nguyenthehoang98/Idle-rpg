using System;
using System.Collections.Generic;
using System.Linq;
using _GameToolkit.GameConfig;
using Newtonsoft.Json;
using ExcelExtension;
using UnityEngine;

namespace _TDS.GameConfig
{
    [Serializable, ExcelAsset(
        ExcelPath = "Assets/Excels/UpgradeConfig.xlsx",
        ConfigPath = "Assets/_TDSAssets/Config/UpgradeConfig.json")]
    public sealed class UpgradeConfig : Config
    {
        [SerializeField, JsonProperty] private List<UpgradeCardConfigData> cards = new List<UpgradeCardConfigData>();

        private List<UpgradeCardConfigData> shopItems;
        private List<UpgradeCardConfigData> heroUpgrades;
        private Dictionary<int, List<UpgradeCardConfigData>> skillPools;

        public override void OnMappingValue()
        {
            shopItems = cards.Where(card => card.kind == UpgradeCardKind.ShopItem).ToList();
            heroUpgrades = cards.Where(card => card.kind == UpgradeCardKind.HeroStat).ToList();
            skillPools = new Dictionary<int, List<UpgradeCardConfigData>>();

            foreach (UpgradeCardConfigData card in cards)
            {
                if (card.kind != UpgradeCardKind.SkillStat || card.skillId <= 0) continue;
                if (!skillPools.TryGetValue(card.skillId, out List<UpgradeCardConfigData> pool))
                {
                    pool = new List<UpgradeCardConfigData>();
                    skillPools.Add(card.skillId, pool);
                }

                pool.Add(card);
            }
        }

        [JsonIgnore] public IReadOnlyList<UpgradeCardConfigData> ShopItems => shopItems;
        [JsonIgnore] public IReadOnlyList<UpgradeCardConfigData> HeroUpgrades => heroUpgrades;

        public IReadOnlyList<UpgradeCardConfigData> GetSkillPool(int skillId)
        {
            return skillPools.TryGetValue(skillId, out List<UpgradeCardConfigData> pool)
                ? pool
                : Array.Empty<UpgradeCardConfigData>();
        }
    }
}
