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
    /// <summary>
    /// Middleware dữ liệu của hệ thống Weapon – cache <see cref="WeaponData"/> và
    /// <see cref="WeaponUpgradeData"/>, liên kết Weapon ↔ Skill, validate trong Editor,
    /// cung cấp API cho Runtime.
    ///
    /// Workflow: LoadConfig → OnMappingValue (cache) → OnValidateLinkConfig (link Weapon↔Skill) → Runtime query.
    /// </summary>
    [Serializable, ExcelAsset(
         ExcelPath = "Assets/Excels/WeaponConfig.xlsx",
         ConfigPath = "Assets/_TDS assets/Config/WeaponConfig.json")]
    public class WeaponConfig : IConfig
    {
        /* ============================== SOURCE DATA (from Excel / JSON) ============================== */

        [SerializeField] private List<WeaponData> weapons = new List<WeaponData>();
        [SerializeField] private List<WeaponUpgradeData> upgrades = new List<WeaponUpgradeData>();

        /// <summary>Weapon đạt từ level này trở lên sẽ là Power X2 (khi không khai báo Power row trong upgrades).</summary>
        [SerializeField] private int powerX2Level = 3;

        /// <summary>Weapon đạt từ level này trở lên sẽ là Power X3.</summary>
        [SerializeField] private int powerX3Level = 5;

        /* ============================== CACHE LOOKUP ============================== */

        private Dictionary<int, WeaponData> cachedWeapons;
        private Dictionary<WeaponUpgradeKey, WeaponUpgradeData> cachedUpgrades;

        // weaponId -> list upgrade (full) ; weaponId -> (level -> list upgrade)  cho Card Upgrade / Runtime
        private Dictionary<int, List<WeaponUpgradeData>> upgradesOfWeapon;
        private Dictionary<int, Dictionary<int, List<WeaponUpgradeData>>> levelUpgradesOfWeapon;

        // weapon -> power upgrades (đã sort theo level) để lấy Power X2 / X3 nhanh
        private Dictionary<int, List<WeaponUpgradeData>> powerUpgradesOfWeapon;

        public IReadOnlyList<WeaponData> AllWeapons => weapons;
        public IReadOnlyList<WeaponUpgradeData> AllUpgrades => upgrades;

        /* ============================== IConfig ============================== */

        public void OnMappingValue()
        {
            CacheWeapons();

            CacheUpgrades();

            RebuildWeaponIndexes();
        }

        public void OnImported()
        {
        }

        /// <summary>Sau khi import, liên kết Weapon ↔ Skill và validate dữ liệu trong Editor.</summary>
        public void OnCompleteImported()
        {
#if UNITY_EDITOR
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

        /* ============================== CACHE ============================== */

        private void CacheWeapons()
        {
            cachedWeapons = new Dictionary<int, WeaponData>();

            foreach (WeaponData data in weapons)
            {
                // TryAdd để báo lỗi duplicate rõ ràng hơn
                if (!cachedWeapons.TryAdd(data.weaponId, data))
                {
                    Debug.LogError($"[WeaponConfig] Duplicate weaponId '{data.weaponId}'");
                }
            }
        }

        private void CacheUpgrades()
        {
            cachedUpgrades = new Dictionary<WeaponUpgradeKey, WeaponUpgradeData>();

            foreach (WeaponUpgradeData data in upgrades)
            {
                WeaponUpgradeKey key = KeyOf(data);

                if (!cachedUpgrades.TryAdd(key, data))
                {
                    Debug.LogError($"[WeaponConfig] Duplicate upgrade {key}");
                }
            }
        }

        private void RebuildWeaponIndexes()
        {
            upgradesOfWeapon = new Dictionary<int, List<WeaponUpgradeData>>();
            levelUpgradesOfWeapon = new Dictionary<int, Dictionary<int, List<WeaponUpgradeData>>>();
            powerUpgradesOfWeapon = new Dictionary<int, List<WeaponUpgradeData>>();

            foreach (WeaponUpgradeData data in upgrades)
            {
                // weapon -> toàn bộ upgrade
                if (!upgradesOfWeapon.TryGetValue(data.weaponId, out List<WeaponUpgradeData> list))
                {
                    list = new List<WeaponUpgradeData>();
                    upgradesOfWeapon.Add(data.weaponId, list);
                }
                list.Add(data);

                // weapon -> level -> upgrades
                if (!levelUpgradesOfWeapon.TryGetValue(data.weaponId, out Dictionary<int, List<WeaponUpgradeData>> levelMap))
                {
                    levelMap = new Dictionary<int, List<WeaponUpgradeData>>();
                    levelUpgradesOfWeapon.Add(data.weaponId, levelMap);
                }

                if (!levelMap.TryGetValue(data.level, out List<WeaponUpgradeData> perLevel))
                {
                    perLevel = new List<WeaponUpgradeData>();
                    levelMap.Add(data.level, perLevel);
                }
                perLevel.Add(data);

                // power upgrades
                if (data.powerLevel > 0)
                {
                    if (!powerUpgradesOfWeapon.TryGetValue(data.weaponId, out List<WeaponUpgradeData> powerList))
                    {
                        powerList = new List<WeaponUpgradeData>();
                        powerUpgradesOfWeapon.Add(data.weaponId, powerList);
                    }
                    powerList.Add(data);
                }
            }
        }

        /* ============================== RUNTIME API ============================== */

        /// <summary>Lấy WeaponData theo weaponId (O(1)).</summary>
        public bool TryGetWeaponData(int weaponId, out WeaponData data)
        {
            if (cachedWeapons != null) return cachedWeapons.TryGetValue(weaponId, out data);
            data = default;
            return false;
        }

        /// <summary>Lấy một upgrade theo (weaponId, level, group, upgradeType) – O(1).</summary>
        public bool TryGetUpgrade(in WeaponUpgradeKey key, out WeaponUpgradeData data)
        {
            if (cachedUpgrades != null) return cachedUpgrades.TryGetValue(key, out data);
            data = null;
            return false;
        }

        /// <summary>Lấy danh sách upgrade (tất cả level) của một weapon. Trả về false nếu không có.</summary>
        public bool TryGetUpgradesWeapon(int weaponId, out List<WeaponUpgradeData> list)
        {
            if (upgradesOfWeapon != null) return upgradesOfWeapon.TryGetValue(weaponId, out list);
            list = null;
            return false;
        }

        /// <summary>Lấy danh sách upgrade khả dụng tại một level của weapon (dùng cho Card Upgrade / Runtime).</summary>
        public bool TryGetUpgradesAtLevel(int weaponId, int level, out List<WeaponUpgradeData> list)
        {
            if (levelUpgradesOfWeapon != null
                && levelUpgradesOfWeapon.TryGetValue(weaponId, out Dictionary<int, List<WeaponUpgradeData>> levelMap))
            {
                return levelMap.TryGetValue(level, out list);
            }

            list = null;
            return false;
        }

        /// <summary>
        /// Sinh Dictionary theo từng Level để Weapon Runtime và Card Upgrade sử dụng.
        /// Map: level → danh sách upgrade khả dụng ở level đó.
        /// </summary>
        public Dictionary<int, List<WeaponUpgradeData>> GetUpgradesLevelWeapon(int weaponId)
        {
            if (levelUpgradesOfWeapon != null && levelUpgradesOfWeapon.TryGetValue(weaponId, out Dictionary<int, List<WeaponUpgradeData>> levelMap))
            {
                return levelMap;
            }

            return new Dictionary<int, List<WeaponUpgradeData>>();
        }

        /// <summary>
        /// Lấy Power X2 / X3 của weapon ở <paramref name="weaponLevel"/>.
        /// Ưu tiên Power row khai báo trong upgrades (có <see cref="WeaponUpgradeData.powerLevel"/> &lt;= level),
        /// nếu không có thì dùng ngưỡng <see cref="powerX2Level"/> / <see cref="powerX3Level"/>.
        /// </summary>
        public bool TryGetUpgradePowerWeapon(int weaponId, int weaponLevel, out int power)
        {
            power = GetPowerLevel(weaponId, weaponLevel);
            return power > 1; // Chỉ coi là "có power" khi &gt;= X2
        }

        /// <summary>Tính Power Level (1, 2 hoặc 3) cho weapon ở level cho trước.</summary>
        public int GetPowerLevel(int weaponId, int weaponLevel)
        {
            // quét power upgrade được khai báo cho weapon: lấy level power cao nhất có level <= weaponLevel
            if (powerUpgradesOfWeapon != null
                && powerUpgradesOfWeapon.TryGetValue(weaponId, out List<WeaponUpgradeData> powerList))
            {
                int best = 0;
                for (int i = 0; i < powerList.Count; i++)
                {
                    WeaponUpgradeData p = powerList[i];
                    if (p.level <= weaponLevel && p.powerLevel > best)
                    {
                        best = p.powerLevel;
                    }
                }

                if (best > 0) return best;
            }

            // Fallback theo ngưỡng cấu hình
            if (weaponLevel >= powerX3Level) return 3;
            if (weaponLevel >= powerX2Level) return 2;
            return 1;
        }

        /// <summary>
        /// Tích lũy toàn bộ upgrade (level &lt;= maxLevel) của một weapon vào bộ modifier runtime.
        /// Dùng để Weapon tính chỉ số cuối khi đạt maxLevel mà không phải query từng upgrade.
        /// </summary>
        public void AccumulateModifiers(int weaponId, int maxLevel, ref WeaponRuntimeModifiers mods)
        {
            if (!TryGetUpgradesWeapon(weaponId, out List<WeaponUpgradeData> list)) return;

            for (int i = 0; i < list.Count; i++)
            {
                WeaponUpgradeData data = list[i];
                if (data.level > maxLevel) continue;
                data.ApplyModifiers(ref mods);
            }
        }

        /* ============================== VALIDATE (Editor) + LINK Weapon ↔ Skill ============================== */

        /// <summary>Load SkillConfig từ JSON đã import (chỉ Editor).</summary>
#if UNITY_EDITOR
        private SkillConfig LoadSkillConfig()
        {
            object[] attrs = typeof(SkillConfig).GetCustomAttributes(typeof(ExcelAssetAttribute), false);
            if (attrs.Length == 0) return null;

            ExcelAssetAttribute attr = (ExcelAssetAttribute)attrs[0];
            TextAsset asset = AssetDatabase.LoadAssetAtPath<TextAsset>(attr.ConfigPath);
            if (asset == null) return null;

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

            for (int i = 0; i < weapons.Count; i++)
            {
                WeaponData data = weapons[i];
                LinkSkillInfo(ref data, skillConfig);
                weapons[i] = data;
            }

            foreach (WeaponUpgradeData data in upgrades)
            {
                ValidateUpgradeKey(data);
            }
        }

        /// <summary>Resolve SkillData cho 1 weapon và copy projectile nếu cần.</summary>
        private void LinkSkillInfo(ref WeaponData data, SkillConfig skillConfig)
        {
            if (!skillConfig.TryGetSkill(data.skillId, out SkillConfigData skillData))
            {
                Debug.LogError($"[WeaponConfig] Weapon '{data.weaponId}' skillId '{data.skillId}' không tồn tại trong SkillConfig.");
                data.skillData = default;
                return;
            }

            data.skillData = skillData;

            // Copy projectile prefab từ Skill nếu weapon chưa khai báo
            if (string.IsNullOrEmpty(data.projectileName))
            {
                data.projectileName = skillData.prefabName;
            }
        }

        private void ValidateUpgradeKey(WeaponUpgradeData data)
        {
            if (!cachedWeapons.ContainsKey(data.weaponId))
            {
                Debug.LogError($"[WeaponConfig] Upgrade weaponId '{data.weaponId}' không tồn tại trong WeaponConfig.");
            }

            if (data.level <= 0)
            {
                Debug.LogWarning($"[WeaponConfig] Upgrade weapon '{data.weaponId}' có level '{data.level}' không hợp lệ (bắt đầu từ 1).");
            }
        }

        /* ============================== HELPERS ============================== */

        private static WeaponUpgradeKey KeyOf(WeaponUpgradeData data)
        {
            return new WeaponUpgradeKey(data.weaponId, data.level, data.group, data.upgradeType);
        }
    }
}