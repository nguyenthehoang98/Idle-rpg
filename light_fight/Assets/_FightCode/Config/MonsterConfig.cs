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
        ConfigPath = "Assets/_Sources/Configs/MonsterConfig.asset")]
    public class MonsterConfig : BaseConfig
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
    public class MonsterData
    {
        [SerializeField] private int id;
        [SerializeField] private int isRanged;
        [SerializeField] private string name;
        [SerializeField] private string path;
        [SerializeField] private int skillId;
        [SerializeField] private int skillLevel;
        [SerializeField] private float skillCooldown;
        [SerializeField] private float radius;
        [SerializeField] private float moveSpeed;
        [SerializeField] private float stopMoveDistance;
        [SerializeField] private float attackDistance;
        [SerializeField] private int attack;
        [SerializeField] private int health;

        public int MonsterId => id;
        public string MonsterName => name;
        public bool IsRanged => isRanged == 1;
        public string Path => path;
        public float Radius => radius;
        public int SkillId => skillId;
        public int SkillLevel => skillLevel;
        public float MoveSpeed => moveSpeed;
        public float SkillCooldown => skillCooldown;
        public float StopMoveDistance => stopMoveDistance;
        public float AttackDistance => attackDistance;
        public int Attack => attack;
        public int Health => health;

        public MonsterData Clone(int newHealth)
        {
            MonsterData a = this;
            return new MonsterData
            {
                id = a.id,
                name = a.name,
                path = a.path,
                isRanged = a.isRanged,
                skillId = a.skillId,
                skillLevel = a.skillLevel,
                skillCooldown = a.skillCooldown,
                moveSpeed = a.moveSpeed,
                stopMoveDistance = a.stopMoveDistance,
                attackDistance = a.attackDistance,
                attack = a.attack,
                health = newHealth,
            };
        }
    }
}