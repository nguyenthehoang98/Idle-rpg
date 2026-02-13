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
        
        public override void OnMapValue()
        {
        }

        public override void OnPostImported()
        {
            foreach (var data in baseData)
            {
                data.OnValidate();
            }
        }

        [Serializable]
        public class MonsterData
        {
            [SerializeField] private int id;
            [SerializeField] private string name;
            [SerializeField] private string address_prefab;
            [SerializeField] private int skill_id; // Monster mặc định kĩ năng level là 1.
            [SerializeField] private float skill_cooldown;
            [SerializeField] private float move_speed;
            [SerializeField] private float attack_range; // Phạm vi tấn công
            [SerializeField] private float base_attack_stat;
            [SerializeField] private float attack_linear;
            [SerializeField] private float attack_rate;
            [SerializeField] private float base_defense_stat;
            [SerializeField] private float defense_linear;
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

            public float AttackRange => attack_range;

            public float Attack(int level) => FormulaUtils.Attack(level, base_attack_stat, attack_linear, attack_rate);
           
            public float Defense(int level) => FormulaUtils.Defense(level, base_defense_stat, defense_linear);
           
            public float Health(int level) => FormulaUtils.Health(level, base_health_stat, health_linear, health_rate); 
            
            public void OnValidate()
            {
                skillId = new SkillId(skill_id, 1);
            }
        }
    }
}
