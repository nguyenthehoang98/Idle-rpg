using System;
using _KIT.Config;
using _KIT.Config.ExcelExtension.Runtime;
using UnityEngine;
namespace _Game.Configs
{
    [ExcelAsset(
        ExcelPath = "Assets/Excels/StatConfig.xlsx",
        ConfigPath = "Assets/AddressableAssetsData/Configs/StatConfig.asset")]
    public class StatConfig : KitBaseConfig
    {
        public override void OnMapValue()
        {
        }
        
        [Serializable]
        public class StatData
        {
            [SerializeField] private StatType type;
            [SerializeField] private float value;
            [SerializeField] private float cp_modifier;
            [SerializeField] private float cp_modifier2;
            
            
        }
    }
}