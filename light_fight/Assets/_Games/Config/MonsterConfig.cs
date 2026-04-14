using System;
using System.Collections.Generic;
using _KIT.Config;
using _KIT.Config.ExcelExtension.Runtime;
using UnityEngine;

namespace _Games.Config
{
    [ExcelAsset(
        ExcelPath = "Assets/Excels/MonsterConfig.xlsx",
        ConfigPath = "Assets/_Sources/Configs/MonsterConfig.asset")]
    public class MonsterConfig : KitBaseConfig
    {
        [SerializeField] private List<MonsterData> monsters = new List<MonsterData>();
    
        private Dictionary<int, MonsterData> cacheMonsterData;
    
#if UNITY_EDITOR
        public static MonsterConfig Instance
        {
            get
            {
                string path = "Assets/_Sources/Configs/MonsterConfig.asset";
                MonsterConfig instance = UnityEditor.AssetDatabase.LoadAssetAtPath<MonsterConfig>(path);
                instance.OnMapValue();
                return instance;
            }
        }
#endif
    
        public override void OnMapValue()
        {
            cacheMonsterData = new Dictionary<int, MonsterData>();
            foreach (var m in monsters)
            {
                cacheMonsterData.Add(m.MonsterId, m);
            }
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