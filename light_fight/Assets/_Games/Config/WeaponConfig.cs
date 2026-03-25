using System;
using System.Collections.Generic;
using _KIT.Config;
using _KIT.Config.ExcelExtension.Runtime;
using UnityEngine;

namespace _Games.Config
{
    [ExcelAsset(
        ExcelPath = "Assets/Excels/WeaponConfig.xlsx",
        ConfigPath = "Assets/_Sources/Configs/WeaponConfig.asset")]
    public class WeaponConfig : KitBaseConfig
    {
        [SerializeField] private List<WeaponData> weapons = new List<WeaponData>();
        private Dictionary<int, WeaponData> cacheWeaponData;
    
        public override void OnMapValue()
        {
            cacheWeaponData = new Dictionary<int, WeaponData>();
            foreach (var m in weapons)
            {
                cacheWeaponData.Add(m.WeaponId, m);
            }
        }
        
        public bool Find(int weaponId, out WeaponData skill)
        {
            return cacheWeaponData.TryGetValue(weaponId, out skill);
        }
    }

    [Serializable]
    public class WeaponData
    {
        [SerializeField] private int weaponId;
        [SerializeField] private string weaponName;
        [SerializeField] private int skillId;
        [SerializeField] private float baseAttack;
        [SerializeField] private float attackBonusLevel;
        [SerializeField] private float basePrice;
        [SerializeField] private float priceBonusLevel;
        [SerializeField] private string[] jsonUnlocks;

        public int WeaponId => weaponId;
        public string WeaponName => weaponName;
        public int SkillId => skillId;
        public float Attack(int level) => baseAttack + attackBonusLevel * level;
        public float Price(int level) => basePrice + priceBonusLevel * level;
        public string[] Unlocks => jsonUnlocks;
    }
}