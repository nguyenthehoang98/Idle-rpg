using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using _KITSystem.Formula;
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
        
#if UNITY_EDITOR
        public override void OnPostImported()
        {
            for (var i = 0; i < monsters.Count; i++)
            {
                MonsterData m = monsters[i];
                ValidateObject<GameObject>(m.path);
                
                StatComplex complex = new StatComplex();
                complex.AddBase(StatType.Attack, m.attack);
                complex.AddBase(StatType.Health, m.health);
                complex.AddBase(StatType.Defense, m.defense);
                complex.AddBase(StatType.AttackSpeed, m.attackSpeed);
                complex.AddBase(StatType.CriticalRate, m.criticalRate);
                complex.AddBase(StatType.CriticalDamage, m.criticalDamage);
                complex.AddBase(StatType.DamageMultiplier, m.damageMultiplier);
                complex.AddBase(StatType.ArmorPenPercent, m.armorPenPercent);

                m.Power = (int)complex.GetPower();
                monsters[i] = m;
            }
        }
#endif

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

        [SerializeField] private int power;

        public int Power
        {
            set => power = value;
        }
    }
}