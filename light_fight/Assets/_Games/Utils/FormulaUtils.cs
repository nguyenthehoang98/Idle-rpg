using _Games.Combat.SkillSystem.Model;
using _KIT.Utils;
using Unity.Mathematics;
using UnityEngine;

namespace _Games.Utils
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

        public static int PowerMonster(MonsterData monsterData, SkillStatData skillStatData)
        {
            int attack = monsterData.Attack;
            int defense = monsterData.Defense;
            int health = monsterData.Health;
            float skillDamage = SkillDamage(attack, skillStatData.ScaleDamage, skillStatData.BaseDamage);
            float dps = DPS(skillDamage, 0, 0);
            float effectiveHp = health * (defense + DEFENSE_K) / DEFENSE_K;
            float power = dps * effectiveHp;
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
        public static int Output(float attack, SkillStatData skillStatData,
            float criticalRate, float criticalDmg, float defense)
        {
            float dmg = SkillDamage(attack, skillStatData.ScaleDamage, skillStatData.BaseDamage);
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
        static float DPS(float skillDmg, float criticalRate, float criticalDmg)
        {
            float criticalFactor = 1f + math.clamp(criticalRate, 0f, 1f) * criticalDmg;
            return skillDmg * criticalFactor;
        }
        
        // =================================================================== //
        
        /// <summary>
        /// Tính giá mỗi lần mua trang bị
        /// </summary>
        public static int Price(int level, int basePrice, int priceLinear)
        {
            return basePrice + level * priceLinear;
        }

        public static int RandomEquipmentLevel(int playerLevel, int currentWave, float bonusRate)
        {
            int maxTier = GetMaxTierByWave(currentWave);

            float waveFactor = currentWave * WAVE_FACTOR_MULTIPLIER;
            float levelFactor = playerLevel * LEVEL_FACTOR_MULTIPLIER;
            float power = waveFactor + levelFactor + bonusRate;

            int[] baseRates = BaseRateEquipment;
            int[] weights = new int[baseRates.Length];

            for (int i = 0; i < baseRates.Length; i++)
            {
                // Hard cap: khóa tier cao
                if (i + 1 > maxTier)
                {
                    weights[i] = 0;
                    continue;
                }

                // Multiplicative scaling
                float scale = 1f + power * ScaleRate[i];
                weights[i] = Mathf.Max(1, Mathf.RoundToInt(baseRates[i] * scale));
            }

            return GetWeightedRandomIndex(weights);
        }
        
        private static int GetWeightedRandomIndex(int[] weights)
        {
            int total = 0;
            for (int i = 0; i < weights.Length; i++)
                total += weights[i];

            int rand = RandomUtils.Range(0, total); // [0, total)

#if DEVELOP_MODE || COMBAT_FULL_LOG
            Debug.Log($"Random weapon level. Picker:{rand}, Weights:[{string.Join(',', weights)}]");
#endif

            int cumulative = 0;
            for (int i = 0; i < weights.Length; i++)
            {
                cumulative += weights[i];
                if (rand < cumulative)
                    return i + 1; // level bắt đầu từ 1
            }

            return weights.Length; // fallback
        }

        private static int GetMaxTierByWave(int wave)
        {
            if (wave < 4) return 2;
            if (wave < 8) return 3;
            if (wave < 12) return 4;
            return 5;
        }
        
        private const float WAVE_FACTOR_MULTIPLIER = 1f;
        private const float LEVEL_FACTOR_MULTIPLIER = 0.5f;
        private static int[] BaseRateEquipment => new int[] { 800, 100, 50, 20, 10 };
        private static float[] ScaleRate = new float[] { 1, 0.6f, 0.4f, 0.25f, 0.1f };
    }
}