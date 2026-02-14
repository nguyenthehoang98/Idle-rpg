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
        ConfigPath = "Assets/AddressableAssetsData/Configs/MonsterConfig.asset")]
    public class MonsterConfig : KitBaseConfig
    {
        [SerializeField] private List<MonsterData> baseData = new List<MonsterData>();
        private Dictionary<int, MonsterData> cacheData;

#if UNITY_EDITOR
        private static MonsterConfig instance;

        public static MonsterConfig Instance
        {
            get
            {
                if(instance == null)
                {
                    string path = "Assets/AddressableAssetsData/Configs/MonsterConfig.asset";
                    instance = UnityEditor.AssetDatabase.LoadAssetAtPath<MonsterConfig>(path);
                }

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
     
        public override void OnPostImported()
        {
            foreach (var data in baseData)
            {
                data.OnValidate();
            }
        }

        public bool Find(int id, out MonsterData value) => cacheData.TryGetValue(id, out value);

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
            [SerializeField] private float skill_cooldown;
            [SerializeField] private float skill_scale_damage;
            [SerializeField] private float skill_flat_damage;
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

            [SerializeField] private SkillId skillId;

            public int ID => id;

            public string Name => name;

            public string AddressPrefab => address_prefab;

            public SkillId SkillId => skillId;

            public float SkillCooldown => skill_cooldown;

            public float MoveSpeed => move_speed;

            public float AttackDistance => attack_distance;

            public float Attack(int level) => FormulaUtils.Attack(level, base_attack_stat, attack_linear, attack_rate);
           
            public float Defense(int level) => FormulaUtils.Defense(level, base_defense_stat, defense_linear, defense_rate);
           
            public float Health(int level) => FormulaUtils.Health(level, base_health_stat, health_linear, health_rate);

            public float SkillDamage(float finalAttack) => FormulaUtils.SkillDamage(finalAttack, skill_scale_damage, skill_flat_damage);
            
            public void OnValidate()
            {
                skillId = new SkillId(skill_id, 1);
            }
        }
    }
}
