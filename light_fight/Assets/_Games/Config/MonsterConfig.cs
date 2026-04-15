using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using _Games.Combat.SkillSystem.Model;
using _KIT.Config;
using _KIT.Config.ExcelExtension.Runtime;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace _Games.Config
{
    [ExcelAsset(
        ExcelPath = "Assets/Excels/MonsterConfig.xlsx",
        ConfigPath = "Assets/_Sources/Configs/MonsterConfig.asset")]
    public class MonsterConfig : KitBaseConfig
    {
        [SerializeField] private List<MonsterData> monsters = new List<MonsterData>();
    
        private Dictionary<int, MonsterData> cacheMonsterData;
    
        public override void OnMapValue()
        {
            cacheMonsterData = new Dictionary<int, MonsterData>();
            foreach (var m in monsters)
            {
                cacheMonsterData.Add(m.MonsterId, m);
            }
        }

        public override void OnPostImported()
        {
#if UNITY_EDITOR
            SkillConfig config = AssetDatabase.LoadAssetAtPath<SkillConfig>("Assets/_Sources/Configs/SkillConfig.asset");
            var field = config.GetType().GetField("skills", BindingFlags.Default | BindingFlags.Instance | BindingFlags.NonPublic);
            var list = field.GetValue(config) as List<SkillData>;
            foreach (var monster in monsters)
            {
                bool exists = list.Any(a => a.SkillId == monster.SkillId);
                if (!exists)
                    Debug.LogError($"[MonsterConfig] Not found skill '{monster.SkillId}' at monster '{monster.MonsterId}'");
            }
#endif
        }

        public bool Find(int monsterId, out MonsterData skill)
        {
            return cacheMonsterData.TryGetValue(monsterId, out skill);
        }
    }

    [Serializable]
    public class MonsterData
    {
        [SerializeField] private int monsterId;
        [SerializeField] private string monsterName;
        [SerializeField] private string monsterObjectId;
        [SerializeField] private int isRanged;
        [SerializeField] private int skillId;
        [SerializeField] private int skillLevel;
        [SerializeField] private float skillCooldown;
        [SerializeField] private float moveSpeed;
        [SerializeField] private float stopMoveDistance;
        [SerializeField] private float attackDistance;
        [SerializeField] private int attack;
        [SerializeField] private int health;

        public int MonsterId => monsterId;
        public string MonsterName => monsterName;
        public bool IsRanged => isRanged == 1;
        public int SkillId => skillId;
        public int SkillLevel => skillLevel;
        public float MoveSpeed => moveSpeed;
        public float SkillCooldown => skillCooldown;
        public float StopMoveDistance => stopMoveDistance;
        public float AttackDistance => attackDistance;
        public int Attack => attack;
        public int Health => health;
    }
}