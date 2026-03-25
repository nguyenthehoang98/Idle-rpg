using System;
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
        public override void OnMapValue()
        {
            
        }
    }

    [Serializable]
    public class WeaponData
    {
        [SerializeField] private int weaponId;
        [SerializeField] private string weaponName;
        [SerializeField] private int skillId;
    }
}