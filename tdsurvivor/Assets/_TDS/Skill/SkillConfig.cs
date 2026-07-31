using System;
using System.Collections.Generic;
using _GameToolkit.GameConfig;
using ExcelExtension;
using UnityEngine;

namespace _TDS.Skill
{
    [Serializable, ExcelAsset(
         ExcelPath = "Assets/Excels/SkillConfig.xlsx",
         ConfigPath = "Assets/_TDS assets/Config/SkillConfig.json")]
    public class SkillConfig : IGameConfig
    {
        [SerializeField] private List<SkillData> skills = new List<SkillData>();

        private Dictionary<int, SkillData> cached;

        public void OnMappingValue()
        {
            cached = new Dictionary<int, SkillData>();

            foreach (SkillData skillData in skills)
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

        public bool TryGetSkill(int skillId, out SkillData skill)
        {
            return cached.TryGetValue(skillId, out skill);
        }
    }
}
