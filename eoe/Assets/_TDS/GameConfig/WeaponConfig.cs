using System;
using System.Collections.Generic;
using _GameToolkit.GameConfig;
using UnityEngine;
#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif

namespace _TDS.GameConfig
{
    [Serializable]
    public class WeaponConfig : IGameConfig
    {
        [SerializeField] private List<WeaponData> weapons = new List<WeaponData>();
        [SerializeField] private List<WeaponUpgradeData> upgrades = new List<WeaponUpgradeData>();

        private Dictionary<int, WeaponData> cachedWeapon;
        private Dictionary<int, WeaponUpgradeData> cachedUpgrade;

        public void OnMappingValue()
        {
            cachedWeapon = new Dictionary<int, WeaponData>();

            foreach (var data in weapons)
            {
                if (!cachedWeapon.TryAdd(data.id, data)) Debug.LogError($"Duplicate weapon '{data.id}'");
            }

            cachedUpgrade = new Dictionary<int, WeaponUpgradeData>();
            foreach (var data in upgrades)
            {
                int key = HashCode.Combine(data.id, data.level, data.group, data.type);
                cachedUpgrade.Add(key, data);
            }
        }

        public void OnImported()
        {
        }

        public void OnCompleteImported()
        {
#if UNITY_EDITOR
            string path = Path.Combine(ConfigPath.Folder, "SkillConfig.json");

            TextAsset asset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
            if (asset == null) return;
            
            SkillConfig skillConfig = JsonUtility.FromJson<SkillConfig>(asset.text);
            
            skillConfig.OnMappingValue();

            for (int i = 0; i < weapons.Count; i++)
            {
                WeaponData weaponData = weapons[i];
                if (skillConfig.TryGetSkillOnEditor(weaponData.skillId, out var skillData))
                {
                    skillData.prefabName = weaponData.projectileName;
                    weaponData.skillData = skillData;
                }
                else Debug.LogError($"Not found skill '{weaponData.skillId}' at weapon '{weaponData.id}'");

                weapons[i] = weaponData;
            }

            foreach (var weaponData in weapons)
            {
                if (string.IsNullOrEmpty(weaponData.prefabName))
                    Debug.LogError($"Prefab name is empty at weapon '{weaponData.id}'");
                if (string.IsNullOrEmpty(weaponData.audioClip))
                    Debug.LogError($"Attack audio is empty at weapon '{weaponData.id}'");
            }
#endif
        }

        public bool TryGetWeaponData(int weaponId, out WeaponData weaponData)
        {
            return cachedWeapon.TryGetValue(weaponId, out weaponData);
        }

        public bool TryGetUpgradePowerWeapon(int weaponId, UpgradeType type, out WeaponUpgradeData data)
        {
            int key = HashCode.Combine(weaponId, 0, 0, type);
            return cachedUpgrade.TryGetValue(key, out data);
        }

        public Dictionary<int, List<WeaponUpgradeData>> GetUpgradesLevelWeapon(int weaponId)
        {
            Dictionary<int, List<WeaponUpgradeData>> dict = new Dictionary<int, List<WeaponUpgradeData>>();
            for (int lv = 1; lv <= 10; lv++)
            {
                for (int gr = 0; gr <= 2; gr++)
                {
                    int key = HashCode.Combine(weaponId, lv, gr, UpgradeType.LevelUp);
                    if (cachedUpgrade.TryGetValue(key, out var data))
                    {
                        if (dict.TryGetValue(lv, out var list)) list.Add(data);
                        else dict.Add(lv, new List<WeaponUpgradeData> { data });
                    }
                }
            }

            return dict;
        }
    }

    [Serializable]
    public struct WeaponData : IEquatable<WeaponData>
    {
        public int id;
        public int skillId;
        public string prefabName;
        public string projectileName;
        public string iconName;
        public string explosivePrefabName;

        public float cooldown;
        public float attackSpeed;
        public int attack;
        public float critChance;
        public float critDamage;

        public SkillData skillData;

        public string audioClip;
        public float volume;

        public override int GetHashCode()
        {
            return 10000 + id;
        }

        public override bool Equals(object obj)
        {
            return obj is WeaponData other && Equals(other);
        }

        public bool Equals(WeaponData other)
        {
            return id == other.id;
        }
    }

    [Serializable]
    public struct WeaponUpgradeData : IEquatable<WeaponUpgradeData>
    {
        public int id;
        public int group;
        public int level;
        public string iconName;
        public UpgradeType type;
        public float attackRange;
        public float attackRate;
        public float projectileScaleBonus;
        public float projectileDamageMultiplier;
        public float cooldownReductionPercent;
        public int projectilesPerShot;
        public int spreadProjectileCount;
        public int bonusPierceCount;
        public float explosiveRadius;
        public float explosiveDamagePercent;
        public float critChance;
        public float critDamage;
        public float executeHealthPercent;

        public override int GetHashCode()
        {
            return HashCode.Combine(id, level, group);
        }

        public bool Equals(WeaponUpgradeData other)
        {
            return id == other.id && group == other.group && level == other.level;
        }

        public override bool Equals(object obj)
        {
            return obj is WeaponUpgradeData other && Equals(other);
        }

        public void Increase(WeaponUpgradeData data)
        {
            attackRange += data.attackRange;
            attackRate += data.attackRate;
            critChance += data.critChance;
            critDamage += data.critDamage;
            executeHealthPercent += data.executeHealthPercent;
            projectileScaleBonus += data.projectileScaleBonus;
            projectileDamageMultiplier += data.projectileDamageMultiplier;
            explosiveDamagePercent += data.explosiveDamagePercent;
            bonusPierceCount += data.bonusPierceCount;
            explosiveRadius += data.explosiveRadius;
            spreadProjectileCount += data.spreadProjectileCount;
            projectilesPerShot += data.projectilesPerShot;
            cooldownReductionPercent += data.cooldownReductionPercent;
        }

        public void Decrease(WeaponUpgradeData data)
        {
            attackRange -= data.attackRange;
            attackRate -= data.attackRate;
            critChance -= data.critChance;
            critDamage -= data.critDamage;
            executeHealthPercent -= data.executeHealthPercent;
            projectileScaleBonus -= data.projectileScaleBonus;
            projectileDamageMultiplier -= data.projectileDamageMultiplier;
            explosiveDamagePercent -= data.explosiveDamagePercent;
            bonusPierceCount -= data.bonusPierceCount;
            explosiveRadius -= data.explosiveRadius;
            spreadProjectileCount -= data.spreadProjectileCount;
            projectilesPerShot -= data.projectilesPerShot;
            cooldownReductionPercent -= data.cooldownReductionPercent;
        }
    }

    public enum UpgradeType
    {
        LevelUp = 0,
        PowerX2,
        PowerX3,
    }
}
