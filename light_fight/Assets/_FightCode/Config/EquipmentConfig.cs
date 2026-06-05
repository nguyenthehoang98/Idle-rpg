using System;
using System.Collections.Generic;
using _KITSystem.Data;
using UnityEngine;
using Newtonsoft.Json;

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
        [JsonProperty] private int Stat_ID;
        [JsonProperty] private int Stat_A;
        [JsonProperty] private int Stat_B;
        [JsonProperty] private int Cur1_ID;
        [JsonProperty] private int Cur1_A;
        [JsonProperty] private int Cur1_B;
        [JsonProperty] private int Cur2_ID;
        [JsonProperty] private int Cur2_A;
        [JsonProperty] private int Cur2_B;

        public int ID;
        [JsonIgnore] public LinearFormula Stat;
        [JsonIgnore] public LinearFormula CUR1;
        [JsonIgnore] public LinearFormula CUR2;

        public void OnImported()
        {
            Stat = new LinearFormula(Stat_ID, Stat_A, Stat_B);
            CUR1 = new LinearFormula(Cur1_ID, Cur1_A, Cur1_B);
            CUR2 = new LinearFormula(Cur2_ID, Cur2_A, Cur2_B);
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