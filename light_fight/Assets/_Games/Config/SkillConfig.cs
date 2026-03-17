using System.Collections.Generic;
using _Games.Combat.SkillSystem.Model;
using _KIT.Config;
using _KIT.Config.ExcelExtension.Runtime;
using UnityEngine;

[ExcelAsset(
    ExcelPath = "Assets/Excels/SkillConfig.xlsx",
    ConfigPath = "Assets/_Sources/Configs/SkillConfig.asset")]
public class SkillConfig : KitBaseConfig
{
    [SerializeField] private List<SkillData> skills = new List<SkillData>();
    [SerializeField] private List<SkillStatData> skillStats = new List<SkillStatData>();

    private Dictionary<int, SkillData> cacheSkillData;
    private Dictionary<string, SkillStatData> cacheSkillStatData;
    
#if UNITY_EDITOR
    public static SkillConfig Instance
    {
        get
        {
            string path = "Assets/_Sources/Configs/SkillConfig.asset";
            SkillConfig instance = UnityEditor.AssetDatabase.LoadAssetAtPath<SkillConfig>(path);
            instance.OnMapValue();
            return instance;
        }
    }
#endif

    
    public override void OnMapValue()
    {
        cacheSkillData = new Dictionary<int, SkillData>();
        foreach (var m in skills)
        {
           cacheSkillData.Add(m.SkillId, m);
        }

        cacheSkillStatData = new Dictionary<string, SkillStatData>();
        foreach (var m in skillStats)
        {
            string key = $"{m.SkillId}_{m.SkillLevel}";
            cacheSkillStatData.Add(key, m);
        }
    }

    public bool Find(int skillId, out SkillData skill)
    {
        return cacheSkillData.TryGetValue(skillId, out skill);
    }

    public bool Find(int skillId, int skillLevel, out SkillStatData skill)
    {
        string key = $"{skillId}_{skillLevel}";
        return cacheSkillStatData.TryGetValue(key, out skill);
    }
}
