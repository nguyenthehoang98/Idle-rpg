using System;
using System.Collections.Generic;
using _Toolkit.Config;
using ExcelExtension;
using UnityEngine;

namespace _TDS.Config
{
    [Serializable, ExcelAsset(
         ExcelPath = "Assets/Excels/SkillConfig.xlsx",
         ConfigPath = "Assets/_TDS assets/Config/SkillConfig.json")]
    public class SkillConfig : IConfig
    {
        [SerializeField] private List<SkillConfigData> skills = new List<SkillConfigData>();

        private Dictionary<int, SkillConfigData> cached;

        public void OnMappingValue()
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

        public void OnImported()
        {
        }

        public void OnCompleteImported()
        {
        }

        public bool TryGetSkill(int skillId, out SkillConfigData skillConfig)
        {
            return cached.TryGetValue(skillId, out skillConfig);
        }
    }
}
