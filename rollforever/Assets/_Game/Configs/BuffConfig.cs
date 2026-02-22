using System;
using System.Collections.Generic;
using _Game.Battle;
using _KIT.Config;
using _KIT.Config.ExcelExtension.Runtime;
using Unity.Mathematics;
using UnityEngine;

namespace _Game.Configs
{
    [ExcelAsset(
        ExcelPath = "Assets/Excels/BuffConfig.xlsx",
        ConfigPath = "Assets/Sources/Configs/BuffConfig.asset")]
    public class BuffConfig : KitBaseConfig
    {
        [SerializeField] private List<BuffData> baseData = new List<BuffData>();
        private Dictionary<int2, List<BuffData>> cacheData;
        
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
            cacheData = new Dictionary<int2, List<BuffData>>();
            foreach (var m in baseData)
            {
                foreach (var skillId in m.SkillIds)
                {
                    int2 key = new int2(skillId, m.BuffLevel);
                    if (cacheData.TryGetValue(key, out List<BuffData> list))
                    {
                        list.Add(m);
                    }
                    else
                    {
                        cacheData.Add(key, new List<BuffData> { m });
                    }
                }
            }
        }

        public bool Find(int buffId, int buffLevel, out List<BuffData> list)
        {
            return cacheData.TryGetValue(new int2(buffId, buffLevel), out list);
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

            public int BuffLevel => buffLevel;

            public StatType StatType => statType;

            public float Value => buffValue;
            
            public IReadOnlyCollection<int> SkillIds => skillIds;
        }
    }
}
