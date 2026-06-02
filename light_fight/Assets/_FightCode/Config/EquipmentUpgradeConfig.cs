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

#if UNITY_EDITOR
        public override void OnPostImported()
        {
            foreach (var data in upgrades)
            {
                if (data.Stat.Length % 2 == 1)
                    Debug.LogError(
                        $"EquipmentUpgradeConfig: Stat Length '{data.Stat.Length}'. Id '{data.ID}', Level '{data.Level}'"
                    );
                
                if (data.CUR.Length % 2 == 1)
                    Debug.LogError(
                        $"EquipmentUpgradeConfig: CUR Length '{data.Stat.Length}'. Id '{data.ID}', Level '{data.Level}'"
                    );
            }
        }
#endif
    }

    [Serializable]
    public struct EquipmentUpgradeData
    {
        public int ID;
        public int Level;
        public int[] CUR; // Tài nguyên xử dụng để upgrade
        public int[] Stat; // Chỉ số stat được + thêm (StatConfig)
    }
}