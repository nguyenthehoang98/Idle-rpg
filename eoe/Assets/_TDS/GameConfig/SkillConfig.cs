using System;
using System.Collections.Generic;
using _GameToolkit.GameConfig;
using ExcelExtension;
using Newtonsoft.Json;
using UnityEngine;

namespace _TDS.GameConfig
{
    [Serializable, ExcelAsset(
         ExcelPath = "Assets/Excels/SkillConfig.xlsx",
         ConfigPath = "Assets/_TDSAssets/Config/SkillConfig.json")]
    public class SkillConfig : Config
    {
        [SerializeField, JsonProperty] private List<SkillConfigData> skills = new List<SkillConfigData>();

        private Dictionary<int, SkillConfigData> cached;

        public override void OnMappingValue()
        {
            cached = new Dictionary<int, SkillConfigData>();

            foreach (SkillConfigData skillData in skills)
            {
                if (!cached.TryAdd(skillData.skillId, skillData))
                {
                    Debug.LogError($"Duplicate skill '{skillData.skillId}'");
                }
            }
        }

        public bool TryGetSkill(int skillId, out SkillConfigData data)
        {
            return cached.TryGetValue(skillId, out data);
        }
    }
}
