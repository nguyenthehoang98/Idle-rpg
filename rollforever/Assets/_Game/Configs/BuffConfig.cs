using System;
using System.Collections.Generic;
using _Game.Battle;
using _KIT.Config;
using _KIT.Config.ExcelExtension.Runtime;
using UnityEngine;

namespace _Game.Configs
{
    [ExcelAsset(
        ExcelPath = "Assets/Excels/BuffConfig.xlsx",
        ConfigPath = "Assets/Sources/Configs/BuffConfig.asset")]
    public class BuffConfig : KitBaseConfig
    {
        [SerializeField] private List<BuffData> baseData = new List<BuffData>();

#if UNITY_EDITOR
        public static SkillConfig Instance
        {
            get
            {
                string path = "Assets/Sources/Configs/BuffConfig.asset";
                SkillConfig instance = UnityEditor.AssetDatabase.LoadAssetAtPath<SkillConfig>(path);
                instance.OnMapValue();
                return instance;
            }
        }
#endif
        
        public override void OnMapValue()
        {
            
        }
        
        [Serializable]
        public class BuffData
        {
            [SerializeField] private int buffId;
            [SerializeField] private int buffLevel;
            [SerializeField] private StatType statType;
            [SerializeField] private float buffValue;
            [SerializeField] private List<int> skillIds;

            public int BuffId => buffId;
        }
    }
}
