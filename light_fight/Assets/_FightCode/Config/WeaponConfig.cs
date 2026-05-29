using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ExcelExtension;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

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
                cacheWeaponData.Add(m.weaponId, m);
            }
        }

#if UNITY_EDITOR
        public override void OnPostImported()
        {
            for (var i = 0; i < weapons.Count; i++)
            {
                ValidateObject<GameObject>(weapons[i].path);
            }
        }
#endif
        
        public bool Find(int weaponId, out WeaponData skill)
        {
            return cacheWeaponData.TryGetValue(weaponId, out skill);
        }
    }

    [Serializable]
    public struct WeaponData
    {
        public int weaponId;
        public string weaponName;
        public string path;
        public int skillId;
        [SerializeField] private float baseAttack;
        [SerializeField] private float attackBonusLevel;
        [SerializeField] private float basePrice;
        [SerializeField] private float priceBonusLevel;

        public int Attack(int level) => (int)(baseAttack + attackBonusLevel * level);
        public int Price(int level) => (int)(basePrice + priceBonusLevel * level);
    }
}