using System;
using System.Collections.Generic;
using _Game.Battle.Utils;
using _KIT.Config;
using _KIT.Config.ExcelExtension.Runtime;
using UnityEngine;

namespace _Game.Scripts.Configs
{
    [ExcelAsset(
        ExcelPath = "Assets/Excels/SkillConfig.xlsx",
        ConfigPath = "Assets/Sources/Configs/SkillConfig.asset")]
    public class SkillConfig : KitBaseConfig
    {
        [SerializeField] private List<SkillData> baseData = new List<SkillData>();
        private Dictionary<int, SkillData> cacheData;

#if UNITY_EDITOR
        public static SkillConfig Instance
        {
            get
            {
                string path = "Assets/Sources/Configs/SkillConfig.asset";
                SkillConfig instance = UnityEditor.AssetDatabase.LoadAssetAtPath<SkillConfig>(path);
                instance.OnMapValue();
                return instance;
            }
        }
#endif
        
        public override void OnMapValue()
        {
            cacheData = new Dictionary<int, SkillData>();
            foreach (var m in baseData)
            {
                cacheData[m.SkillId] = m;
            }
        }
        
        public bool Find(int id, out SkillData value) => cacheData.TryGetValue(id, out value);
        
        [Serializable]
        public class SkillData
        {
            [SerializeField] private int skillId;
            [SerializeField] private float skill_cooldown;
            [SerializeField] private float skill_scale_damage;
            [SerializeField] private float skill_flat_damage;

            public int SkillId => skillId;
            
            public float SkillCooldown => skill_cooldown;
            
            public float SkillDamage(float finalAttack) => FormulaUtils.SkillDamage(finalAttack, skill_scale_damage, skill_flat_damage);
        }
    }
}