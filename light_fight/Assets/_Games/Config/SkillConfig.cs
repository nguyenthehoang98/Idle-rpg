using System;
using System.Collections.Generic;
using ExcelExtension;
using UnityEngine;

namespace _Games.Config
{
    [ExcelAsset(
        ExcelPath = "Assets/Excels/SkillConfig.xlsx",
        ConfigPath = "Assets/_Sources/Configs/SkillConfig.asset")]
    public class SkillConfig : BaseConfig
    {
        [SerializeField] private List<SkillData> skills = new List<SkillData>();
        
        private Dictionary<int, SkillData> cacheSkillData;
    
        public override void OnMapValue()
        {
            cacheSkillData = new Dictionary<int, SkillData>();
            foreach (var m in skills)
            {
                cacheSkillData.Add(m.SkillId, m);
            }
        }

        public bool Find(int skillId, out SkillData skill)
        {
            return cacheSkillData.TryGetValue(skillId, out skill);
        }
    }
    
    [Serializable]
    public class SkillData
    {
        [SerializeField] private int id;
        [SerializeField] private int level;
        [SerializeField] private string path;
        [SerializeField] private string name;
        [SerializeField] private float baseFlatDamage;
        [SerializeField] private float flatDamageBonusLevel;
        [SerializeField] private float baseScaleDamage;
        [SerializeField] private float scaleDamageBonusLevel;
        
        public float FlatDamage(int lv) => baseFlatDamage + lv * flatDamageBonusLevel;
        public float ScaleDamage(int lv) => baseScaleDamage + lv * scaleDamageBonusLevel;

        public int SkillId => id;

        public int Level => level;

        public string Path => path;
    }
}
