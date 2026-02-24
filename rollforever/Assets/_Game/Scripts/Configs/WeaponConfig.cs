using System;
using System.Collections.Generic;
using _Game.Battle.Utils;
using _Game.Scripts.Weapon;
using _KIT.Config;
using _KIT.Config.ExcelExtension.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Game.Scripts.Configs
{
    [ExcelAsset(
        ExcelPath = "Assets/Excels/WeaponConfig.xlsx",
        ConfigPath = "Assets/_Sources/Configs/WeaponConfig.asset")]
    public class WeaponConfig : KitBaseConfig
    {
        [SerializeField] private List<WeaponData> baseData = new List<WeaponData>();
        private Dictionary<int, WeaponData> cacheData;

#if UNITY_EDITOR
        public static WeaponConfig Instance
        {
            get
            {
                string path = "Assets/_Sources/Configs/WeaponConfig.asset";
                WeaponConfig instance = UnityEditor.AssetDatabase.LoadAssetAtPath<WeaponConfig>(path);
                instance.OnMapValue();
                return instance;
            }
        }
#endif

        public override void OnMapValue()
        {
            cacheData = new Dictionary<int, WeaponData>();
            foreach (var m in baseData)
            {
                cacheData[m.WeaponId] = m;
            }
        }

#if UNITY_EDITOR
        public override void OnPostImported()
        {
            // todo: validate weapon so
            string folder = "Assets/_Sources/Battles/Weapons";
            UnityEngine.Object[] objects = AssetDatabase.LoadAllAssetsAtPath(folder);
            foreach (var weaponData in baseData)
            {
                bool found = false;
                foreach (var o in objects)
                {
                    if (o.name == weaponData.WeaponId.ToString())
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    Debug.LogError($"Not found file WeaponSO with id '{weaponData.WeaponId}'");
                }
            }

            foreach (var obj in objects)
            {
                if (obj is WeaponSO weaponSo)
                {
                    if(weaponSo.WeaponIcon == null)
                    {
                        Debug.LogError("WeaponIcon is null, at file WeaponSO: " + AssetDatabase.GetAssetPath(obj));
                    }
                }
                else Debug.LogError($"Object {AssetDatabase.GetAssetPath(obj)} not defined is WeaponSO");
            }
        }
#endif

        public bool Find(int monsterId, out WeaponData value)
        {
            return cacheData.TryGetValue(monsterId, out value);
        }

        [Serializable]
        public class WeaponData
        {
            [FormerlySerializedAs("weapon_id")] [SerializeField] private int weaponWeaponID;
            [SerializeField] private string name;
            [SerializeField] private int skill_id;
            [SerializeField] private float base_attack_stat;
            [SerializeField] private float attack_linear;
            [SerializeField] private float attack_rate;
            [SerializeField] private string jsonUnlockLv5;
            [SerializeField] private string jsonUnlockLv10;
            [SerializeField] private string jsonUnlockLv15;
            [SerializeField] private string jsonUnlockLv20;
            [SerializeField] private string jsonUnlockLv25;
            [SerializeField] private string jsonUnlockLv30;

            public int WeaponId => weaponWeaponID;
            public string Name => name;
            public int SkillId => skill_id;
            public float Attack(int level) => FormulaUtils.Attack(level, base_attack_stat, attack_linear, attack_rate);
        }
    }
}