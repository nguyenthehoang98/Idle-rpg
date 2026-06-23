using System;
using System.Collections.Generic;
using _KITSystem.Config;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace _Game.Configs
{
    [Serializable]
    public class WeaponConfig : IGameConfig
    {
        [SerializeField] private List<WeaponData> weapons = new List<WeaponData>();
        [SerializeField] private List<WeaponUpgradeData> upgrades = new List<WeaponUpgradeData>();

        private Dictionary<int, WeaponData> cachedWeapon;
        private Dictionary<int, List<WeaponUpgradeData>> cachedUpgrade;
        
        public void OnMappingValue()
        {
            cachedWeapon = new Dictionary<int, WeaponData>();

            foreach (var data in weapons)
            {
                if (!cachedWeapon.TryAdd(data.id, data)) Debug.LogError($"Duplicate weapon '{data.id}'");
            }

            cachedUpgrade = new Dictionary<int, List<WeaponUpgradeData>>();
            foreach (var data in upgrades)
            {
                int key = HashCode.Combine(data.id, data.level, data.type);
                if (cachedUpgrade.TryGetValue(key, out var list))
                {
                    list.Add(data);
                }
                else cachedUpgrade.Add(key, new List<WeaponUpgradeData> { data });
            }
        }

        public void OnPostImported()
        {
        }

        public void OnValidateLinkConfig()
        {
#if UNITY_EDITOR
            TextAsset asset = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/_BattleSource/Configs/SkillConfig.json");
            SkillConfig skillConfig = JsonUtility.FromJson<SkillConfig>(asset.text);
            skillConfig.OnMappingValue();

            for (int i = 0; i < weapons.Count; i++)
            {
                WeaponData weaponData = weapons[i];
                if (skillConfig.TryGetSkillOnEditor(weaponData.skillId, out var skillData))
                {
                    weaponData.skillData = skillData;
                    weaponData.skillData.prefabName = weaponData.projectileName;
                }
                else Debug.LogError($"Not found skill '{weaponData.skillId}' at weapon '{weaponData.id}'");
                weapons[i] = weaponData;
            }
#endif
        }

        public bool TryGetWeaponData(int weaponId, out WeaponData weaponData)
        {
            return cachedWeapon.TryGetValue(weaponId, out weaponData);
        }

        public bool TryGetUpgradeWeapon(int weaponId, int level, UpgradeType type, out List<WeaponUpgradeData> list)
        {
            int key = HashCode.Combine(weaponId, level, type);
            return cachedUpgrade.TryGetValue(key, out list);
        }
    }
    
    [Serializable]
    public struct WeaponData
    {
        public int id;
        public string prefabName;
        public string projectileName;
        public int skillId;
        public float cooldown;
        public float attackSpeed;
        public int attack;
        public float critChance;
        public float critDamage;
        public SkillData skillData;
        public string attackAudioClip;
        public float attackVolume;
    }

    [Serializable]
    public struct WeaponUpgradeData
    {
        public int id;
        public int level;
        public UpgradeType type;
        public float attackSpeed;
        public float projectileSize;
        public float damagePercent;
        public float cooldownReduce;
        public int parallelCount;
        public int spreadCount;
        public float spreadDamagePercent;
        public int piercingCount;
        public float explosiveRadius;
        public float explosiveDamagePercent;
        public float critChance;
        public float critDamage;
        public int bounceCount;
        public float bonceDamagePercent;
        public float killInstantBelowHealthPercent;
    }

    public enum UpgradeType
    {
        LevelUp = 0,
        PowerX2,
        PowerX3,
    }
}