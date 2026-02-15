using _Game.Configs;
using _KIT.Utils;
using Unity.Mathematics;
using UnityEngine;

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
        private const float DEFENSE_K = 1000;

        public static int PowerMonster(MonsterConfig.MonsterData monsterData, int level)
        {
            float attack = monsterData.Attack(level);
            float defense = monsterData.Defense(level);
            float health = monsterData.Health(level);
            float skillDamage = monsterData.SkillDamage(attack);
            float dps = DPS(skillDamage, monsterData.SkillCooldown, 0, 0);
            float effectiveHp = health * (defense + DEFENSE_K) / DEFENSE_K;
            float power = dps * effectiveHp;
            /*Debug.Log($"Monster:{monsterData.ID}, Level: {level}, " +
                      $"\nATK:{attack}, DEF:{defense}, HP:{health}, " +
                      $"\nDPS:{dps}, EffectiveHp:{effectiveHp}, Power:{(int)math.sqrt(power)}" +
                      $"\n");*/
            return (int)math.sqrt(power);
        }

        /*
         *  Early game: gần như tuyến tính
         *  Mid game: bắt đầu cong lên
         *  Late game: tăng cực nhanh do exponential dominate
         */
        public static float Attack(int level, float baseValue, float valueLinear, float rate)
        {
            int m = math.max(1, level) - 1;
            return (baseValue + valueLinear * m) * math.pow(rate, m);
        }

        /*
         * Ổn định
         * Dễ kiểm soát
         * Không snowball
         */
        public static float Defense(int level, float baseValue, float valueLinear, float rate)
        {
            int m = math.max(1, level) - 1;
            return (baseValue + valueLinear * m) * math.pow(rate, m);
        }
        
        /*
         * Tăng exponential nhưng chậm hơn ATK
         * Vì ATK nhân cả linear trong exponential
         */
        public static float Health(int level, float baseValue, float valueLinear, float rate)
        {
            int m = math.max(1, level) - 1;
            return (baseValue + valueLinear * m) * math.pow(rate, m);
        }

        /// <summary>
        /// Tính sát thương của kĩ năng
        /// </summary>
        /// <param name="attack"></param>
        /// <param name="skillScaleDamage"></param>
        /// <param name="skillFlatDamage"></param>
        /// <returns></returns>
        public static float SkillDamage(float attack, float skillScaleDamage, float skillFlatDamage)
        {
            return attack * skillScaleDamage + skillFlatDamage;
        }
        
        /// <summary>
        /// Hàm tính sát thương -> kẻ dịch
        /// </summary>
        /// <param name="attack">Sát thương dạng chỉ số của nhân vật</param>
        /// <param name="skillScaleDmg">Sát thương % của kĩ năng</param>
        /// <param name="skillFlatDmg">Sát thương truc tiep của kĩ năng</param>
        /// <param name="criticalRate">Tỉ lệ có thể critical chung</param>
        /// <param name="criticalDmg">Sát thương crtitical tăng thêm</param>
        /// <param name="defense">Thủ của mục tiêu</param>
        /// <returns></returns>
        public static int Output(float attack, float skillScaleDmg, float skillFlatDmg,
            float criticalRate, float criticalDmg, float defense)
        {
            float dmg = SkillDamage(attack, skillScaleDmg, skillFlatDmg);
            bool crit = criticalRate >= RandomUtils.Value;
            float totalDmg = crit ? dmg * (1 + math.max(0, criticalDmg)) : dmg;

            float armorPenPercent = 0;
            float effectiveDef = defense * (1f - armorPenPercent);
            float reduce = effectiveDef / (effectiveDef + DEFENSE_K);
            int outputDmg = (int)(totalDmg * (1 - reduce));
            return outputDmg;
        }

        /// <summary>
        ///  Chỉ số thể hiện khẳ năng gây sát thương  
        /// </summary>
        /// <returns></returns>
        static float DPS(float skillDmg, float cooldown, float criticalRate, float criticalDmg)
        {
            float atkSpeed = cooldown > 0 ? 1f / cooldown : 0f;
            float criticalFactor = 1f + math.clamp(criticalRate, 0f, 1f) * criticalDmg;
            return skillDmg * atkSpeed * criticalFactor;
        }
    }
}