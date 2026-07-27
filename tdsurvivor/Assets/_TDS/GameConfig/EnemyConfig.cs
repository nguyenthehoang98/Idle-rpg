using System;
using System.Collections.Generic;
using _GameToolkit.GameConfig;
using ExcelExtension;
using UnityEngine;

namespace _TDS.GameConfig
{
    [Serializable, ExcelAsset(
         ExcelPath = "Assets/Excels/EnemyConfig.xlsx",
         ConfigPath = "Assets/_TDS assets/Resources/EnemyConfig.json")]
    public class EnemyConfig : IGameConfig
    {
        [SerializeField] private List<EnemyConfigData> enemies = new List<EnemyConfigData>();
        
        Dictionary<int, EnemyConfigData> cachedEnemies;
        
        public void OnMappingValue()
        {
            cachedEnemies = new Dictionary<int, EnemyConfigData>();

            foreach (var enemy in enemies)
            {
                cachedEnemies.Add(enemy.id, enemy);
            }
        }

        public void OnImported()
        {
        }

        public void OnCompleteImported()
        {
        }

        public bool TryGetMonster(int id, out EnemyConfigData config)
        {
            return cachedEnemies.TryGetValue(id, out config);
        }
    }

    [Serializable]
    public struct EnemyConfigData
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