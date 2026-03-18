using System;
using System.Collections.Generic;
using _Games.Combat.SkillSystem.Model;
using _KIT.Config;
using _KIT.Config.ExcelExtension.Runtime;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

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

    public override void OnPostImported()
    {
        HashSet<string> keys = new HashSet<string>();
        keys.Add(string.Empty);
        keys.Add("null");
        keys.Add("Null");
        List<(string, string)> keyValuePairs = new List<(string, string)>();
        foreach (var skillData in skills)
        {
            if (keys.Add(skillData.ProjectileId)) keyValuePairs.Add((skillData.ProjectileId, "projectile"));
            if (keys.Add(skillData.ColliderId)) keyValuePairs.Add((skillData.ColliderId, "collider"));
            if (keys.Add(skillData.TrajectoryId)) keyValuePairs.Add((skillData.TrajectoryId, "trajectory"));
            foreach (var m in skillData.ModifiersId)
            {
                if (keys.Add(m)) keyValuePairs.Add((m, "modifier"));
            }
            foreach (var b in skillData.BehaviorsId)
            {
                if (keys.Add(b)) keyValuePairs.Add((b, "behavior"));
            }
        }

        foreach (var pair in keyValuePairs)
        {
            Load(pair.Item1, pair.Item2);
        }
    }

    private void Load(string path, string type)
    {
        if (string.IsNullOrEmpty(path)) return;
        try
        {
            Addressables.LoadAssetAsync<ScriptableObject>(path).Completed += handle =>
            {
                if (handle.Status != AsyncOperationStatus.Succeeded)
                    Debug.LogError($"[SkillConfig] Not found [{type.ToUpper()}] with name: " + path);
            };
        }
        catch (Exception e)
        {
            Debug.LogError($"[SkillConfig] Not found [{type.ToUpper()}] with name: " + path);
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
