using System;
using System.Collections.Generic;
using System.Linq;
using _GameToolkit.GameConfig;
using ExcelExtension;
using Newtonsoft.Json;
using UnityEngine;
using _TDS.Battle;

namespace _TDS.GameConfig
{
    [Serializable, ExcelAsset(
         ExcelPath = "Assets/Excels/SkillConfig.xlsx",
         ConfigPath = "Assets/_TDSAssets/Config/SkillConfig.json")]
    public class SkillConfig : Config
    {
        [SerializeField, JsonProperty] private List<SkillConfigData> skills = new List<SkillConfigData>();
        [SerializeField, JsonProperty] private List<UpgradeCardConfigData> upgrade = new List<UpgradeCardConfigData>();

        private Dictionary<int, SkillConfigData> cached;
        private List<UpgradeCardConfigData> shopItems;
        private List<UpgradeCardConfigData> heroUpgrades;
        private Dictionary<int, List<UpgradeCardConfigData>> skillPools;
        private Dictionary<int, UpgradeCardConfigData> statUpgradePools;

        public override void OnMappingValue()
        {
            cached = new Dictionary<int, SkillConfigData>();
            shopItems = upgrade.Where(card => card.kind == UpgradeCardKind.ShopItem).ToList();
            heroUpgrades = upgrade.Where(card => card.kind == UpgradeCardKind.HeroStat).ToList();
            skillPools = new Dictionary<int, List<UpgradeCardConfigData>>();
            statUpgradePools = new Dictionary<int, UpgradeCardConfigData>();

            foreach (SkillConfigData skillData in skills)
            {
                if (!cached.TryAdd(skillData.skillId, skillData))
                {
                    Debug.LogError($"Duplicate skill '{skillData.skillId}'");
                }
            }

            foreach (UpgradeCardConfigData card in upgrade)
            {
                if (card.kind == UpgradeCardKind.PowerStat)
                {
                    if (card.id <= 0 || card.heroStat == StatId.None || card.value == 0f)
                    {
                        continue;
                    }

                    if (!statUpgradePools.TryAdd(card.id, card))
                    {
                        Debug.LogError($"Duplicate stat upgrade id '{card.id}'");
                    }

                    continue;
                }

                if (card.kind != UpgradeCardKind.SkillStat || card.skillId <= 0) continue;
                if (!skillPools.TryGetValue(card.skillId, out List<UpgradeCardConfigData> pool))
                {
                    pool = new List<UpgradeCardConfigData>();
                    skillPools.Add(card.skillId, pool);
                }

                pool.Add(card);
            }
        }

        public bool TryGetSkill(int skillId, out SkillConfigData data)
        {
            return cached.TryGetValue(skillId, out data);
        }

        public IReadOnlyList<StatUpgradeConfigData> GetStatUpgrades(IReadOnlyList<int> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return Array.Empty<StatUpgradeConfigData>();
            }

            List<StatUpgradeConfigData> result = new List<StatUpgradeConfigData>(ids.Count);
            for (int i = 0; i < ids.Count; i++)
            {
                if (!statUpgradePools.TryGetValue(ids[i], out UpgradeCardConfigData upgradeData))
                {
                    continue;
                }

                result.Add(new StatUpgradeConfigData
                {
                    id = upgradeData.id,
                    stat = upgradeData.heroStat,
                    value = upgradeData.value,
                    percent = upgradeData.percent,
                });
            }

            return result;
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
