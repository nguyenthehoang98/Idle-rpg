using System;
using System.Collections.Generic;
using _GameToolkit.GameConfig;
using ExcelExtension;
using Unity.Mathematics;
using UnityEngine;

namespace _TDS.GameConfig
{
    [ExcelAsset(
        ExcelPath = "Assets/Excels/SkillConfig.xlsx",
        ConfigPath = "Assets/_TDS assets/Resources/SkillConfig.json")]
    public class SkillConfig : IGameConfig
    {
        [SerializeField] private List<SkillData> baseData = new List<SkillData>();
        
        private Dictionary<int2, SkillData> cacheData;

        public void OnMappingValue()
        {
            cacheData = new Dictionary<int2, SkillData>();
            foreach (var m in baseData)
            {
                int2 key = new int2(m.SkillId, m.Level);
                cacheData.Add(key, m);
            }
        }

        public void OnImported()
        {
            Debug.Log("OnImported");
        }

        public void OnCompleteImported()
        {
            Debug.Log("OnCompleteImported");
        }
        
        public bool Find(int skillId, int level, out SkillData value) => cacheData.TryGetValue(new int2(skillId, level), out value);
        
        [Serializable]
        public class SkillData
        {
            [SerializeField] private int skill_id;
            [SerializeField] private int level;
            [SerializeField] private string skill_path;
            [SerializeField] private string skill_name;
            [SerializeField] private float skill_cooldown;
            [SerializeField] private float scale_damage;

            public int SkillId => skill_id;
            public int Level => level;
            public string SkillPath => skill_path;
        }
    }
}