using System;
using System.Collections.Generic;
using ExcelExtension;
using UnityEngine;

namespace _FightCode.Config
{
    [ExcelAsset(
        ExcelPath = "Assets/Excels/MonsterClassConfig.xlsx",
        ConfigPath = "Assets/_FightSource/Configs/MonsterClassConfig.asset")]
    public class MonsterClassConfig : BaseConfig
    {
        [SerializeField] private List<MonsterClassData> classes = new List<MonsterClassData>();
    
        private Dictionary<int, MonsterClassData> byId;
    
        public override void OnMapValue()
        {
            byId = new Dictionary<int, MonsterClassData>();

            foreach (var data in classes)
            {
                byId.Add(data.ID, data);
            }
        }

        public bool TryGetMonsterClassByID(int classID, out MonsterClassData monsterClassData)
        {
            return byId.TryGetValue(classID, out monsterClassData);
        }
    }


    [Serializable]
    public struct MonsterClassData
    {
        public int ID;
        public float HPScale;
        public float AttackScale;
        public float CooldownIntervalScale;
        public float SpeedScale;
    }
}