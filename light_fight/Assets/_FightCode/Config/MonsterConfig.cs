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
        public List<MonsterData> monsters = new List<MonsterData>();
    
        private Dictionary<int, MonsterData> _cacheMonsterData;
    
        public override void OnMapValue()
        {
            _cacheMonsterData = new Dictionary<int, MonsterData>();
            foreach (var m in monsters)
            {
                _cacheMonsterData.Add(m.monsterId, m);
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
                bool exists = list.Any(a => a.SkillId == monster.skillId);
                if (!exists)
                    Debug.LogError($"[MonsterConfig] Not found skill '{monster.skillId}' at monster '{monster.monsterId}'");
                ValidateObject<GameObject>(monster.path);
            }
        }
#endif

        public bool Find(int monsterId, out MonsterData skill)
        {
            return _cacheMonsterData.TryGetValue(monsterId, out skill);
        }
    }

    [Serializable]
    public struct MonsterData
    {
        public int monsterId;
        public string monsterName;
        public string path;
        public int isRanged;
        public int skillId;
        public int skillLevel;
        public float radius;
        public float moveSpeed;
        public float stopMoveDistance;
        public float attackDistance;
        public float attack;
        public float health;
        public float defense;
        public float attackSpeed;
        public float criticalRate;
        public float criticalDamage;
        public float damageMultiplier;
        public float armorPenPercent;
    }
}