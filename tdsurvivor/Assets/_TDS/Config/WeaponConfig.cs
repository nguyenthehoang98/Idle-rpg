using System;
using System.Collections.Generic;
using _Toolkit.Config;
using ExcelExtension;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace _TDS.Config
{
    [Serializable, ExcelAsset(
         ExcelPath = "Assets/Excels/WeaponConfig.xlsx",
         ConfigPath = "Assets/_TDS assets/Config/WeaponConfig.json")]
    public class WeaponConfig : IConfig
    {
        [SerializeField] private List<WeaponData> weapons = new List<WeaponData>();
        [SerializeField] private List<WeaponUpgradeData> upgrades = new List<WeaponUpgradeData>();

        private Dictionary<int, WeaponData> cachedWeapons;
        private Dictionary<int, Dictionary<int, List<WeaponUpgradeData>>> cachedUpgrades;

        public void OnMappingValue()
        {
            cachedWeapons = new Dictionary<int, WeaponData>();
            foreach (var w in weapons)
            {
                if (!cachedWeapons.TryAdd(w.weaponId, w)) Debug.LogError($"WeaponConfig couldn't be added to weapon '{w.weaponId}'");
            }

            cachedUpgrades = new Dictionary<int, Dictionary<int, List<WeaponUpgradeData>>>();
            foreach (var u in upgrades)
            {
                if (cachedUpgrades.TryGetValue(u.weaponId, out var dict))
                {
                    if (dict.TryGetValue(u.level, out var dict2))
                        dict2.Add(u);
                    else dict.Add(u.level, new List<WeaponUpgradeData> { u });
                }
                else
                {
                    cachedUpgrades.Add(u.weaponId, new Dictionary<int, List<WeaponUpgradeData>>
                    {
                        { u.level, new List<WeaponUpgradeData> { u } }
                    });
                }
            }
        }

        public void OnImported()
        {
        }

        public void OnCompleteImported()
        {
#if UNITY_EDITOR
            // Importer chỉ gọi OnCompleteImported, không gọi OnMappingValue -> cachedWeapons null.
            // Build cache trước để ValidateUpgradeKey so sánh đúng weaponId.
            OnMappingValue();

            SkillConfig skillConfig = LoadSkillConfig();

            if (skillConfig != null)
            {
                OnValidateLinkConfig(skillConfig);
            }
            else
            {
                Debug.LogError("[WeaponConfig] Không load được SkillConfig. Skip link Weapon ↔ Skill.");
            }
#endif
        }

        public bool TryGetWeaponData(int weaponId, out WeaponData data)
        {
            if (cachedWeapons != null) return cachedWeapons.TryGetValue(weaponId, out data);
            data = default;
            return false;
        }

        public bool TryGetUpgradesAtLevel(int weaponId, int level, out List<WeaponUpgradeData> list)
        {
            list = null;
            return cachedUpgrades.TryGetValue(weaponId, out var dict) && dict.TryGetValue(level, out list);
        }

#if UNITY_EDITOR
        private SkillConfig LoadSkillConfig()
        {
            object[] attrs = typeof(SkillConfig).GetCustomAttributes(typeof(ExcelAssetAttribute), false);
            if (attrs.Length == 0) return null;

            ExcelAssetAttribute attr = (ExcelAssetAttribute)attrs[0];
            TextAsset asset = AssetDatabase.LoadAssetAtPath<TextAsset>(attr.ConfigPath);
            if (asset == null)
            {
                Debug.LogError($"Not found SkillConfig at '{attr.ConfigPath}'");
                return null;
            }

            SkillConfig config = JsonUtility.FromJson<SkillConfig>(asset.text);
            config.OnMappingValue();
            return config;
        }
#endif

        /// <summary>
        /// Liên kết Weapon ↔ Skill: resolve skillId → SkillData, copy projectile nếu WeaponData thiếu,
        /// rồi validate lần lượt từng phần (tách thành hàm nhỏ cho rõ ràng).
        /// </summary>
        public void OnValidateLinkConfig(SkillConfig skillConfig)
        {
            if (skillConfig == null) return;

            foreach (WeaponUpgradeData data in upgrades)
            {
                if (cachedWeapons == null || !cachedWeapons.ContainsKey(data.weaponId))
                {
                    Debug.LogError($"[WeaponConfig] Upgrade weaponId '{data.weaponId}' không tồn tại trong WeaponConfig.");
                }

                if (data.level <= 0)
                {
                    Debug.LogWarning($"[WeaponConfig] Upgrade weapon '{data.weaponId}' có level '{data.level}' không hợp lệ (bắt đầu từ 1).");
                }
            }
        }
    }
}