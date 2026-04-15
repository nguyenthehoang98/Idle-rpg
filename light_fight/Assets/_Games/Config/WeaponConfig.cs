using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using _KIT.Config.ExcelExtension.Runtime;
using UnityEngine;
using _Games.Combat.SkillSystem.Model;
using _Games.Utils;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace _Games.Config
{
    [ExcelAsset(
        ExcelPath = "Assets/Excels/WeaponConfig.xlsx",
        ConfigPath = "Assets/_Sources/Configs/WeaponConfig.asset")]
    public class WeaponConfig : BaseConfig
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

#if UNITY_EDITOR
        public override void OnPostImported()
        {
            SkillConfig config = AssetDatabase.LoadAssetAtPath<SkillConfig>("Assets/_Sources/Configs/SkillConfig.asset");
            var field = config.GetType().GetField("skills", BindingFlags.Default | BindingFlags.Instance | BindingFlags.NonPublic);
            var list = field.GetValue(config) as List<SkillData>;
            foreach (var weapon in weapons)
            {
                bool exists = list.Any(a => a.SkillId == weapon.SkillId);
                if (!exists)
                    Debug.LogError($"[WeaponConfig] Not found skill '{weapon.SkillId}' at weapon '{weapon.WeaponId}'");
                Load<GameObject>(GlobalsPath.GetWeaponItemPath(weapon.WeaponId));
                Load<ScriptableObject>(GlobalsPath.GetWeaponSOPath(weapon.WeaponId));
            }
        }
#endif

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
        public int Attack(int level) => (int)(baseAttack + attackBonusLevel * level);
        public int Price(int level) => (int)(basePrice + priceBonusLevel * level);
        public string[] Unlocks => jsonUnlocks;
    }
}