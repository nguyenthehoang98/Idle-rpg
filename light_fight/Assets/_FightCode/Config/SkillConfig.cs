using System;
using System.Collections.Generic;
using ExcelExtension;
using UnityEngine;
using UnityEngine.Serialization;

namespace _FightCode.Config
{
    [ExcelAsset(
        ExcelPath = "Assets/Excels/SkillConfig.xlsx",
        ConfigPath = "Assets/_FightSource/Configs/SkillConfig.asset")]
    public class SkillConfig : BaseConfig
    {
        [SerializeField] private List<SkillData> skills = new List<SkillData>();

        private Dictionary<int, SkillData> cacheSkillData;

        public override void OnMapValue()
        {
            cacheSkillData = new Dictionary<int, SkillData>();
            foreach (var m in skills)
            {
                cacheSkillData.Add(m.skillId, m);
            }
        }

        public bool Find(int skillId, out SkillData skill)
        {
            return cacheSkillData.TryGetValue(skillId, out skill);
        }
    }

    [Serializable]
    public struct SkillData
    {
        public int skillId;
        public string skillName;
        public string path;
        [SerializeField] private float baseFlatDamage;
        [SerializeField] private float flatDamageBonusLevel;
        [SerializeField] private float baseScaleDamage;
        [SerializeField] private float scaleDamageBonusLevel;

        public float FlatDamage(int level) => baseFlatDamage + (level - 1) * flatDamageBonusLevel;
        public float ScaleDamage(int level) => baseScaleDamage + (level - 1) * scaleDamageBonusLevel;
    }
}