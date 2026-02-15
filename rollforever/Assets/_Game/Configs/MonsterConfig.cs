using System;
using System.Collections.Generic;
using _Game.Battle;
using _KIT.Config;
using _KIT.Config.ExcelExtension.Runtime;
using UnityEngine;

namespace _Game.Configs
{
    [ExcelAsset(
        ExcelPath = "Assets/Excels/MonsterConfig.xlsx",
        ConfigPath = "Assets/Sources/Configs/MonsterConfig.asset")]
    public class MonsterConfig : KitBaseConfig
    {
        [SerializeField] private List<MonsterData> baseData = new List<MonsterData>();
        private Dictionary<int, MonsterData> cacheData;

#if UNITY_EDITOR
        public static MonsterConfig Instance
        {
            get
            {
                string path = "Assets/Sources/Configs/MonsterConfig.asset";
                MonsterConfig instance = UnityEditor.AssetDatabase.LoadAssetAtPath<MonsterConfig>(path);
                instance.OnMapValue();
                return instance;
            }
        }
#endif

        public override void OnMapValue()
        {
            cacheData = new Dictionary<int, MonsterData>();
            foreach (var m in baseData)
            {
                cacheData[m.ID] = m;
            }
        }

        public bool Find(int monsterId, out MonsterData value)
        {
            return cacheData.TryGetValue(monsterId, out value);
        }

        [Serializable]
        public class MonsterData
        {
            // Kết hợp = 100:01:01 
            // 100: cố định
            // 01: Hệ số sức mạnh số to thì là mạnh
            // 01: Skin nhân vật (nhân vật cùng chỉ số nhưng mà khác skin)
            [SerializeField] private int id;
            [SerializeField] private string name;
            [SerializeField] private string address_prefab;
            [SerializeField] private int skill_id; // Monster mặc định kĩ năng level là 1.
            [SerializeField] private float move_speed;
            [SerializeField] private float attack_distance; // Phạm vi tấn công
            [SerializeField] private float base_attack_stat;
            [SerializeField] private float attack_linear;
            [SerializeField] private float attack_rate;
            [SerializeField] private float base_defense_stat;
            [SerializeField] private float defense_linear;
            [SerializeField] private float defense_rate;
            [SerializeField] private float base_health_stat;
            [SerializeField] private float health_linear;
            [SerializeField] private float health_rate;

            public int ID => id;
            public string Name => name;
            public string AddressPrefab => address_prefab;
            public int SkillId => skill_id;
            public float MoveSpeed => move_speed;
            public float AttackDistance => attack_distance;

            public float Attack(int level) => FormulaUtils.Attack(level, base_attack_stat, attack_linear, attack_rate);
            public float Defense(int level) => FormulaUtils.Defense(level, base_defense_stat, defense_linear, defense_rate);
            public float Health(int level) => FormulaUtils.Health(level, base_health_stat, health_linear, health_rate);
        }
    }
}
