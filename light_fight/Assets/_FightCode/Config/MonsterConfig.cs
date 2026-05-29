using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ExcelExtension;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace _FightCode.Config
{
    [ExcelAsset(
        ExcelPath = "Assets/Excels/MonsterConfig.xlsx",
        ConfigPath = "Assets/_FightSource/Configs/MonsterConfig.asset")]
    public class MonsterConfig : BaseConfig
    {
        [SerializeField] private List<MonsterData> monsters = new List<MonsterData>();
    
        private Dictionary<int, MonsterData> cacheMonsterData;
    
        public override void OnMapValue()
        {
            cacheMonsterData = new Dictionary<int, MonsterData>();

            foreach (var m in monsters)
            {
                cacheMonsterData.Add(m.monsterId, m);
            }
        }

        public bool Find(int monsterId, out MonsterData skill)
        {
            return cacheMonsterData.TryGetValue(monsterId, out skill);
        }
    }

    [Serializable]
    public struct MonsterData
    {
        public int monsterId;
        public string monsterName;
        public string path;
        public int skillId;
        public int skillLevel;
        public float radius;
        public float moveSpeed;
        public float stopMoveDistance;
        public float attack;
        public float health;
        public float defense;
        public float attackSpeed;
        public float criticalRate;
        public float criticalDamage;
        public float damageMultiplier;
        public float armorPenPercent;

        public int power;
    }
}