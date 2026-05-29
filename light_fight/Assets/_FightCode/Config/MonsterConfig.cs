using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ExcelExtension;
using UnityEditor;
using UnityEngine;

namespace _FightCode.Config
{
    [ExcelAsset(
        ExcelPath = "Assets/Excels/MonsterConfig.xlsx",
        ConfigPath = "Assets/_FightSource/Configs/MonsterConfig.asset")]
    public class MonsterConfig : BaseConfig
    {
        public List<MonsterData> monsters = new List<MonsterData>();
    
        private Dictionary<int, MonsterData> cacheMonsterData;
    
        public override void OnMapValue()
        {
            cacheMonsterData = new Dictionary<int, MonsterData>();
            foreach (var m in monsters)
            {
                cacheMonsterData.Add(m.MonsterId, m);
            }
        }

#if UNITY_EDITOR
        public override void OnPostImported()
        {
            SkillConfig config = AssetDatabase.LoadAssetAtPath<SkillConfig>("Assets/_Sources/Configs/SkillConfig.asset");
            var field = config.GetType().GetField("skills", BindingFlags.Default | BindingFlags.Instance | BindingFlags.NonPublic);
            var list = field.GetValue(config) as List<SkillData>;
            foreach (var monster in monsters)
            {
                bool exists = list.Any(a => a.SkillId == monster.SkillId);
                if (!exists)
                    Debug.LogError($"[MonsterConfig] Not found skill '{monster.SkillId}' at monster '{monster.MonsterId}'");
                ValidateObject<GameObject>(monster.Path);
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
        [Header("Core")]
        public int id;
        public string name;
        public string path;
        public int isRanged;
        [Header("Skill")]
        public int skillId;
        public int skillLevel;
        [Header("Runtime config")]
        public float radius;
        public float moveSpeed;
        public float stopMoveDistance;
        public float attackDistance;
        [Header("Stat")]
        public int attack;
        public int health;
        public int defense;
        public int attackSpeed; // 0~100
        public int criticalRate; // 0~100
        public int criticalDamage; // 0~100
        public int damageMultiplier; // 0~100
        public int armorPenPercent; // 0

        public int MonsterId => id;
        public string MonsterName => name;
        public bool IsRanged => isRanged == 1;
        public string Path => path;
        public float Radius => radius;
        public int SkillId => skillId;
        public int SkillLevel => skillLevel;
        public float MoveSpeed => moveSpeed;
        public float StopMoveDistance => stopMoveDistance;
        public float AttackDistance => attackDistance;
        public int Attack => attack;
        public int Health => health;
        public int Defense => defense;
        public int AttackSpeed => attackSpeed;
        public int CriticalRate => criticalRate;
        public int CriticalDamage => criticalDamage;
        public int DamageMultiplier => damageMultiplier;
        public int ArmorPenPercent => armorPenPercent;
    }
}