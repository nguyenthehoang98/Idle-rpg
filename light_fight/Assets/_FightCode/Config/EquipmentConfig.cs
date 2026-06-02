using System;
using System.Collections.Generic;
using ExcelExtension;
using UnityEngine;

namespace _FightCode.Config
{
    [ExcelAsset(
        ExcelPath = "Assets/Excels/EquipmentConfig.xlsx",
        ConfigPath = "Assets/_FightSource/Configs/EquipmentConfig.asset")]
    public class EquipmentConfig : BaseConfig
    {
        [SerializeField] private List<EquipmentData> equipments = new List<EquipmentData>();
        
        private Dictionary<int, EquipmentData> byId;
        private Dictionary<int, List<EquipmentData>> byGroupId;
    
        public override void OnMapValue()
        {
            byId = new Dictionary<int, EquipmentData>();
            byGroupId = new Dictionary<int, List<EquipmentData>>();
            
            foreach (var m in equipments)
            {
                byId.Add(m.ID, m);

                if (byGroupId.TryGetValue(m.GroupID, out var list))
                    list.Add(m);
                else
                    byGroupId.Add(m.GroupID, new List<EquipmentData> { m });
            }
        }

#if UNITY_EDITOR
        public override void OnPostImported()
        {
            for (var i = 0; i < equipments.Count; i++)
            {
                ValidateObject<GameObject>(equipments[i].PrefabName);
            }
        }
#endif
        
        public bool TryGetEquipmentById(int ID, out EquipmentData equipmentData)
        {
            return byId.TryGetValue(ID, out equipmentData);
        }

        public bool TryGetEquipmentByGroupId(int groupID, out List<EquipmentData> equipmentsData)
        {
            return byGroupId.TryGetValue(groupID, out equipmentsData);
        }
    }

    [Serializable]
    public struct EquipmentData
    {
        public int ID;
        public int GroupID;
        public string NameKey;
        public string PrefabName;
        public string IconName;
        public EquipmentType Type;
        public EquipmentRarity Rarity;
        public int RarityLevel;         // chưa rõ
        public int MaxLevel;            // level nâng cấp tối đa
        public int ActiveSkill;         // kĩ năng active
        public int PassiveSkill;        // kĩ năng passive
        public int Upgrade;             // id của cột EquipmentUpgradeData
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