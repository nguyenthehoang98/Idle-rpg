using System;
using System.Collections.Generic;
using ExcelExtension;
using UnityEngine;

namespace _FightCode.Config
{
    [ExcelAsset(
        ExcelPath = "Assets/Excels/EquipmentUpgradeConfig.xlsx",
        ConfigPath = "Assets/_FightSource/Configs/EquipmentUpgradeConfig.asset")]
    public class EquipmentUpgradeConfig : BaseConfig
    {
        [SerializeField] private List<EquipmentUpgradeData> upgrades = new List<EquipmentUpgradeData>();

        Dictionary<int, EquipmentUpgradeData> byIdLevel;
        
        public override void OnMapValue()
        {
            
        }
    }

    [Serializable]
    public struct EquipmentUpgradeData
    {
        public int ID;
        public int StatID; // query từ StatConfig
        [SerializeField] private int StatValueA; 
        [SerializeField] private int StatValueB;

        public int StatValue(int level)
        {
            return StatValueA + level * StatValueB;
        }
    }
}