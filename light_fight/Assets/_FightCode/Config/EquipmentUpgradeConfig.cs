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
        [SerializeField] private int CURID1;
        [SerializeField] private int CURValueA1;
        [SerializeField] private int CURValueA2;
        [SerializeField] private int CURID2;
        [SerializeField] private int CURValueB1;
        [SerializeField] private int CURValueB2;

        public int StatValue(int level)
        {
            return StatValueA + level * StatValueB;
        }
    }
}