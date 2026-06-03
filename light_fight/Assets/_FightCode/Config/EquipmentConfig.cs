using System;
using System.Collections.Generic;
using _KITSystem.Data;
using UnityEngine;

namespace _FightCode.Config
{
    [Serializable]
    public class EquipmentConfig : IGameConfig
    {
        [SerializeField] private List<EquipmentUpgradeData> UpgradeConfigs_1 = new List<EquipmentUpgradeData>();
        [SerializeField] private List<EquipmentData> Overview = new List<EquipmentData>();
        
        private Dictionary<int, EquipmentData> byId;
        private Dictionary<int, List<EquipmentData>> byGroupId;
    
        public void OnMappingValue()
        {
        }

        public void OnPostImported()
        {
            for (var i = 0; i < UpgradeConfigs_1.Count; i++)
            {
                var data = UpgradeConfigs_1[i];
                data.OnImported();
                UpgradeConfigs_1[i] = data;
            }
        }
    }

    [Serializable]
    public struct EquipmentData 
    {
        public int ID;
        public int GroupID;
        public string Name;
        public string Prefab;
        public string Icon;
        public int MaxLevel;           
        public int UpgradeID;            
        public EquipmentType Type;
        public EquipmentRarity Rarity;
        public int ActiveSkillID;        
        public int PassiveSkillID;  
    }
    
    [Serializable]
    public struct EquipmentUpgradeData 
    {
        public int ID;
        [SerializeField, HideInInspector] public string Stats; 
        [SerializeField, HideInInspector] public string CUR;

        public LinearFormula StatFormula;
        public LinearFormula CUR1Formula;
        public LinearFormula CUR2Formula;
        
        public void OnImported()
        {
            if(!string.IsNullOrEmpty(Stats))
            {
                string[] split = Stats.Trim('[', ']').Split(',');
                if (split.Length == 3)
                    StatFormula = new LinearFormula(int.Parse(split[0]), int.Parse(split[1]), int.Parse(split[2]));
                else Debug.LogError("Stats is invalid " + ID + ", length " + split.Length);
            }
            else Debug.LogError("Stats is invalid " + ID);
            
            if(!string.IsNullOrEmpty(CUR))
            {
                string[] split = CUR.Trim('[', ']').Split(',');
                if (split.Length == 6)
                {
                    CUR1Formula = new LinearFormula(int.Parse(split[0]), int.Parse(split[1]), int.Parse(split[2]));
                    CUR2Formula = new LinearFormula(int.Parse(split[3]), int.Parse(split[4]), int.Parse(split[5]));
                }
                else Debug.LogError("CUR is invalid " + ID + ", length " + split.Length);
            }
            else Debug.LogError("CUR is invalid " + ID);
        }
    }

    public enum EquipmentType
    {
        Weapon, Body, Head, Accessory
    }

    public enum EquipmentRarity
    {
        Common, Uncommon, Rare, Epic, Legendary, Mystic
    }
}