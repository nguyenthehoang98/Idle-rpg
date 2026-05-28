using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ExcelExtension;
using UnityEditor;
using UnityEngine;

namespace _FightCode.Config
{
    [ExcelAsset(
        ExcelPath = "Assets/Excels/WeaponConfig.xlsx",
        ConfigPath = "Assets/_FightSource/Configs/WeaponConfig.asset")]
    public class WeaponConfig : BaseConfig
    {
        [SerializeField] private List<WeaponData> weapons = new List<WeaponData>();
        private Dictionary<int, WeaponData> cacheWeaponData;
    
        public override void OnMapValue()
        {
            cacheWeaponData = new Dictionary<int, WeaponData>();
            foreach (var m in weapons)
            {
                cacheWeaponData.Add(m.Id, m);
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
        [SerializeField] private int id;
        [SerializeField] private string name;
        [SerializeField] private string path;
        [SerializeField] private int skillId;
        [SerializeField] private float baseAttack;
        [SerializeField] private float attackBonusLevel;
        [SerializeField] private float basePrice;
        [SerializeField] private float priceBonusLevel;
        [SerializeField] private string[] jsonUnlocks;

        public int Id => id;
        public string Name => name;
        public string Path => path;
        public int SkillId => skillId;
        public int Attack(int level) => (int)(baseAttack + attackBonusLevel * level);
        public int Price(int level) => (int)(basePrice + priceBonusLevel * level);
        public string[] Unlocks => jsonUnlocks;
    }
}