using System;
using System.Collections.Generic;
using System.Text;
using _KITSystem.Config;
using K4os.Compression.LZ4;
using UnityEngine;
using UnityEngine.Serialization;
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

        public void OnPostImported()
        {
        }

        public void OnValidateLinkConfig()
        {
#if UNITY_EDITOR
            TextAsset asset = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/_BattleSource/Configs/SkillConfig.json");
            
            byte[] unpick = LZ4Pickler.Unpickle(asset.bytes); 

            string text = Encoding.UTF8.GetString(unpick);
            
            SkillConfig skillConfig = JsonUtility.FromJson<SkillConfig>(text);
            
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
                if (string.IsNullOrEmpty(weaponData.projectileName))
                    Debug.LogError($"Projectile name is empty at weapon '{weaponData.id}'");
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
        [SerializeField] private int i;

        public int id
        {
            get => i;
            set => i = value;
        }

        [SerializeField] private int si;

        public int skillId
        {
            get => si;
            set => si = value;
        }

        [SerializeField] private string pn;

        public string prefabName
        {
            get => pn;
            set => pn = value;
        }

        [SerializeField] private string pn2;

        public string projectileName
        {
            get => pn2;
            set => pn2 = value;
        }

        [SerializeField] private string in2;

        public string iconName
        {
            get => in2;
            set => in2 = value;
        }

        /*
         * @ Stat
         */
        [SerializeField] private float cd;

        public float cooldown
        {
            get => cd;
            set => cd = value;
        }

        [SerializeField] private float as2;

        public float attackSpeed
        {
            get => as2;
            set => as2 = value;
        }

        [SerializeField] private int a2;

        public int attack
        {
            get => a2;
            set => a2 = value;
        }

        [SerializeField] private float cc2;

        public float critChance
        {
            get => cc2;
            set => cc2 = value;
        }

        [SerializeField] private float cd2;

        public float critDamage
        {
            get => cd2;
            set => cd2 = value;
        }

        /// <summary>
        /// Runtime data
        /// </summary>

        [SerializeField] private SkillData ssr;

        public SkillData skillData
        {
            get => ssr;
            set => ssr = value;
        }

        /// <summary>
        /// Attack audio clip
        /// </summary>
        [SerializeField] private string mp3;

        public string audioClip
        {
            get => mp3;
            set => mp3 = value;
        }

        /// <summary>
        /// Attack volume
        /// </summary>
        [SerializeField] private float v;

        public float volume
        {
            get => v;
            set => v = value;
        }

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
        [SerializeField] private int i;

        public int id
        {
            get => i;
            set => i = value;
        }

        [SerializeField] private int g;

        public int group
        {
            get => g;
            set => g = value;
        }

        [SerializeField] private int l;

        public int level
        {
            get => l;
            set => l = value;
        }

        [SerializeField] string in2;

        public string iconName
        {
            get => in2;
            set => in2 = value;
        }

        [SerializeField] private UpgradeType t;

        public UpgradeType type
        {
            get => t;
            set => t = value;
        }

        [SerializeField] private float as2;

        public float attackSpeed
        {
            get => as2;
            set => as2 = value;
        }

        [SerializeField] private float ps2;

        public float projectileSize
        {
            get => ps2;
            set => ps2 = value;
        }

        [SerializeField] private float dp;

        public float damagePercent
        {
            get => dp;
            set => dp = value;
        }

        [SerializeField] private float cdr;

        public float cooldownReduce
        {
            get => cdr;
            set => cdr = value;
        }

        [SerializeField] private int pc;

        public int parallelCount
        {
            get => pc;
            set => pc = value;
        }

        [SerializeField] private float pdp;

        public float parallelDamagePercent
        {
            get => pdp;
            set => pdp = value;
        }

        [SerializeField] private int sc;

        public int spreadCount
        {
            get => sc;
            set => sc = value;
        }

        [SerializeField] private float sdp;

        public float spreadDamagePercent
        {
            get => sdp;
            set => sdp = value;
        }

        [SerializeField] private int pc2;

        public int piercingCount
        {
            get => pc2;
            set => pc2 = value;
        }

        [SerializeField] private float er;

        public float explosiveRadius
        {
            get => er;
            set => er = value;
        }

        [SerializeField] private float edp;

        public float explosiveDamagePercent
        {
            get => edp;
            set => edp = value;
        }

        [SerializeField] private float cc;

        public float critChance
        {
            get => cc;
            set => cc = value;
        }

        [SerializeField] private float cd;

        public float critDamage
        {
            get => cd;
            set => cd = value;
        }

        [SerializeField] private int bc;

        public int bounceCount
        {
            get => bc;
            set => bc = value;
        }

        [SerializeField] private float bdp;

        public float bounceDamagePercent
        {
            get => bdp;
            set => bdp = value;
        }

        [SerializeField] private float kibhp;

        public float killInstantBelowHealthPercent
        {
            get => kibhp;
            set => kibhp = value;
        }

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
            attackSpeed += data.attackSpeed;
            critChance += data.critChance;
            critDamage += data.critDamage;
            bounceCount += data.bounceCount;
            bounceDamagePercent += data.bounceDamagePercent;
            killInstantBelowHealthPercent += data.killInstantBelowHealthPercent;
            projectileSize += data.projectileSize;
            parallelDamagePercent += data.parallelDamagePercent;
            damagePercent += data.damagePercent;
            explosiveDamagePercent += data.explosiveDamagePercent;
            spreadDamagePercent += data.spreadDamagePercent;
            piercingCount += data.piercingCount;
            explosiveRadius += data.explosiveRadius;
            spreadCount += data.spreadCount;
            parallelCount += data.parallelCount;
            cooldownReduce += data.cooldownReduce;
        }

        public void Decrease(WeaponUpgradeData data)
        {
            attackSpeed -= data.attackSpeed;
            critChance -= data.critChance;
            critDamage -= data.critDamage;
            bounceCount -= data.bounceCount;
            parallelDamagePercent -= data.parallelDamagePercent;
            bounceDamagePercent -= data.bounceDamagePercent;
            killInstantBelowHealthPercent -= data.killInstantBelowHealthPercent;
            projectileSize -= data.projectileSize;
            damagePercent -= data.damagePercent;
            explosiveDamagePercent -= data.explosiveDamagePercent;
            spreadDamagePercent -= data.spreadDamagePercent;
            piercingCount -= data.piercingCount;
            explosiveRadius -= data.explosiveRadius;
            spreadCount -= data.spreadCount;
            parallelCount -= data.parallelCount;
            cooldownReduce -= data.cooldownReduce;
        }
    }

    public enum UpgradeType
    {
        LevelUp = 0,
        PowerX2,
        PowerX3,
    }
}