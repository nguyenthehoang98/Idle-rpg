using System;
using System.Collections.Generic;
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
        [SerializeField, JsonProperty] private List<StatUpgradeConfigData> upgrade = new List<StatUpgradeConfigData>();

        private Dictionary<int, SkillConfigData> cached;
        private Dictionary<int, StatUpgradeConfigData> statUpgradePools;

        public override void OnMappingValue()
        {
            cached = new Dictionary<int, SkillConfigData>();
            statUpgradePools = new Dictionary<int, StatUpgradeConfigData>();

            foreach (SkillConfigData skillData in skills)
            {
                if (!cached.TryAdd(skillData.skillId, skillData))
                {
                    Debug.LogError($"Duplicate skill '{skillData.skillId}'");
                }
            }

            foreach (StatUpgradeConfigData upgradeData in upgrade)
            {
                if (upgradeData.id <= 0 || upgradeData.stat == StatId.None || upgradeData.value == 0f)
                {
                    continue;
                }

                if (!statUpgradePools.TryAdd(upgradeData.id, upgradeData))
                {
                    Debug.LogError($"Duplicate stat upgrade id '{upgradeData.id}'");
                }
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
                if (statUpgradePools.TryGetValue(ids[i], out StatUpgradeConfigData upgradeData))
                {
                    result.Add(upgradeData);
                }
            }

            return result;
        }
    }
}
