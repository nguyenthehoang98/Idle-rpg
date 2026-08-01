using System;
using System.Collections.Generic;
using _Toolkit.Config;
using ExcelExtension;
using UnityEngine;

namespace _TDS.GameConfig
{
    [Serializable, ExcelAsset(
         ExcelPath = "Assets/Excels/MonsterConfig.xlsx",
         ConfigPath = "Assets/_TDS assets/Config/MonsterConfig.json")]
    public class MonsterConfig : IConfig
    {
        [SerializeField] private List<MonsterConfigData> monsters = new List<MonsterConfigData>();
        
        Dictionary<int, MonsterConfigData> cachedMonsters;
        
        public void OnMappingValue()
        {
            cachedMonsters = new Dictionary<int, MonsterConfigData>();

            foreach (var m in monsters)
            {
                cachedMonsters.Add(m.id, m);
            }
        }

        public void OnImported()
        {
        }

        public void OnCompleteImported()
        {
        }

        public bool TryGetMonster(int id, out MonsterConfigData config)
        {
            return cachedMonsters.TryGetValue(id, out config);
        }
    }

    [Serializable]
    public struct MonsterConfigData
    {
        public int id;
        public string asset;
        public string name;
        public int health;
        public int attack;
        public int exp;
        public float moveSpeed;
        public float stopDistance;
    }
}