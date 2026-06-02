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

        private Dictionary<int, EquipmentUpgradeData> byId;
        
        public override void OnMapValue()
        {
            byId = new Dictionary<int, EquipmentUpgradeData>();

            foreach (var upgradeData in upgrades)
            {
                byId.Add(upgradeData.ID, upgradeData);
            }
        }

        public bool TryGetUpgrade(int id, out EquipmentUpgradeData upgrade)
        {
            return byId.TryGetValue(id, out upgrade);
        }
    }

    [Serializable]
    public struct EquipmentUpgradeData
    {
        public int ID;
        public int StatID; // query từ StatConfig
        [SerializeField] private int StatValueA; 
        [SerializeField] private int StatValueB;
        public int CURID1;
        [SerializeField] private int CURValueA1;
        [SerializeField] private int CURValueB1;
        public int CURID2;
        [SerializeField] private int CURValueA2;
        [SerializeField] private int CURValueB2;

        public LinearFormula StatFormula
        {
            get => new LinearFormula(StatValueA, StatValueB);
        }

        public LinearFormula CUR1Formula
        {
            get => new LinearFormula(CURValueA1, CURValueB1);
        }

        public LinearFormula CUR2Formula
        {
            get => new LinearFormula(CURValueA2, CURValueB2);
        }
    }
}