using System;
using System.Collections.Generic;
using _Game.Battle.Utils;
using _KIT.Config;
using _KIT.Config.ExcelExtension.Runtime;
using UnityEngine;

namespace _Game.Scripts.Configs
{
    [ExcelAsset(
        ExcelPath = "Assets/Excels/WeaponConfig.xlsx",
        ConfigPath = "Assets/Sources/Configs/WeaponConfig.asset")]
    public class WeaponConfig : KitBaseConfig
    {
        [SerializeField] private List<WeaponData> baseData = new List<WeaponData>();
        private Dictionary<int, WeaponData> cacheData;

#if UNITY_EDITOR
        public static WeaponConfig Instance
        {
            get
            {
                string path = "Assets/Sources/Configs/WeaponConfig.asset";
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
                cacheData[m.ID] = m;
            }
        }

        public bool Find(int monsterId, out WeaponData value)
        {
            return cacheData.TryGetValue(monsterId, out value);
        }

        [Serializable]
        public class WeaponData
        {
            [SerializeField] private int weaponId;
            [SerializeField] private string name;
            [SerializeField] private string address_prefab;
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

            public int ID => weaponId;
            public string Name => name;
            public string AddressPrefab => address_prefab;
            public int SkillId => skill_id;
            public float Attack(int level) => FormulaUtils.Attack(level, base_attack_stat, attack_linear, attack_rate);
        }
    }
}