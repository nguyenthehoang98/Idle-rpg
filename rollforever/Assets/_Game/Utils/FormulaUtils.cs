using _Game.Configs;
using _KIT.Utils;
using Unity.Mathematics;

namespace _Game.Battle
{
    /*
     * Quy chuẩn về time hết
     * TTK_out = EnemyHP / PlayerDPS
     * TTK_in  = PlayerEHP / EnemyDPS
     * => Mọi stat bạn thiết kế phải phục vụ kiểm soát 2 con số này.
     
     * Phục vụ tính power:
     * DPS = ATK × AttackSpeed × CritFactor × Multiplier
     * CritFactor = 1 + CritChance × CritDamageBonus

     * Các chỉ số tăng Multiplicative explosion chứ không Linear * tuyến tính

     * DPS = ATK × (1 + ATK%) × (1 + C × CD)

     * DamageReduction = DEF / (DEF + K)
     * (Effective HP) EHP = HP / (1 - DamageReduction)

     * HitsToKill = ceil(HP / Damage)
     */
   
    public static class FormulaUtils
    {
        private const float DEFENSE_K = 100;
        
        public static int PowerSkill(SkillId skillId, float attack, float defense, float health)
        {
            return 1;
        }

        public static int PowerMonster(int monsterId, int monsterLevel)
        {
            return 0;
        }
        
        public static int PowerMonster(MonsterConfig.MonsterData monsterData, int level)
        {
            float attack = monsterData.Attack(level);
            float defense = monsterData.Defense(level);
            float health = monsterData.Health(level);

            float skillPower = PowerSkill(monsterData.SkillId, attack, defense, health);
            float skillDps = skillPower / monsterData.SkillCooldown;
            float effective = health * (100f + health) / 100f;
            float power = skillDps * effective;
            return (int)math.sqrt(power);
        }
        
        public static float Attack(int level, float baseValue, float valueLinear, float rate)
        {
            level = math.max(1, level);
            int m = level - 1;
            return (baseValue + valueLinear * m) * math.pow(rate, m);
        }

        public static float Defense(int level, float baseValue, float valueLinear)
        {
            level = math.max(1, level);
            int m = level - 1;
            return (baseValue + valueLinear * m);
        }
        
        public static float Health(int level, float baseValue, float valueLinear, float rate)
        {
            level = math.max(1, level);
            int m = level - 1;
            return baseValue * math.pow(rate, m) + valueLinear * m;
        }

        // Mở rộng: xuyên giáp, crit, ...
        public static int Output(float attack, float skillScaleDmg, float skillFlatDmg,
            float criticalRate, float criticalDmg, float defense)
        {
            float totalDmg = attack * skillScaleDmg + skillFlatDmg;
            bool crit = criticalRate >= RandomUtils.Value;
            float dmg = crit ? totalDmg * (1 + math.max(0, criticalDmg)) : totalDmg;

            float armorPenPercent = 0;
            float effectiveDef = defense * (1f - armorPenPercent);
            float reduce = effectiveDef / (effectiveDef + DEFENSE_K);
            return (int) (dmg * (1 - reduce));
        }
    }
}