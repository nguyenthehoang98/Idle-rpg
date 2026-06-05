using System;
using System.Collections.Generic;

namespace _FightCode.Config
{
    internal static class Formula
    {
        /// ==================================================================
        /// DAMAGE: Player → Monster (or PvP with casterElementalCounterMod = 0)
        /// ==================================================================
        public static float Damage(float baseDamage, float casterGearPower,
            float casterAttack, float casterGearAttack,
            bool isCrit, float casterCritDamageBonus, float casterGearCritDamageBonus,
            float casterElementalCounterMod,
            float casterElementalPenetration, float casterGearElementalPenetration,
            float casterElementalDmgMultiplier, float casterGearElementalDmgMultiplier,
            float targetElementResistance,
            float intensity)
        {
            float attack = casterAttack * casterGearAttack;

            float critMul = 1f;
            if (isCrit)
                critMul += casterCritDamageBonus + casterGearCritDamageBonus;

            float res = Math.Clamp(targetElementResistance, 0f, 1f);
            float eleFactor = (1f + casterElementalCounterMod + casterElementalPenetration + casterGearElementalPenetration)
                            * (1f + casterElementalDmgMultiplier + casterGearElementalDmgMultiplier)
                            * (1f - res);

            return baseDamage * casterGearPower * attack * critMul * eleFactor * intensity;
        }

        /// ==================================================================
        /// DAMAGE: Monster → Player (defense / resistance / dodge)
        /// ==================================================================
        public static float DamageMonsterToPlayer(float baseDamage,
            float casterAttack,
            float targetDefense,
            float targetElementResistance,
            float targetAttackTypeResistance,
            float casterElementalDmgMultiplier,
            float targetDodgeFactor, // 0 = dodged, 1 = hit
            float intensity)
        {
            float defFactor = 1f - targetDefense;
            float eleResFactor = 1f - targetElementResistance;
            float atkTypeFactor = 1f - targetAttackTypeResistance;
            float eleDmgFactor = 1f + casterElementalDmgMultiplier;

            return baseDamage * casterAttack
                 * defFactor * eleResFactor * atkTypeFactor
                 * eleDmgFactor * targetDodgeFactor * intensity;
        }

        /// ==================================================================
        /// DAMAGE: Flat subtraction (DamageReduction) + clamp min ≥ 1
        /// ==================================================================
        public static float ApplyDamageReduction(float rawDamage, float damageReduction)
        {
            float result = rawDamage - damageReduction;
            if (result < 0f) result = 0f;

            if (result > 0f && result < 1f)
                result = 1f;

            return result;
        }

        /// ==================================================================
        /// SHIELD: Absorb damage by shield, return remaining damage
        /// ==================================================================
        public static float ApplyShield(float damage, float currentShield, out float remainingShield)
        {
            if (currentShield <= 0f)
            {
                remainingShield = 0f;
                return damage;
            }

            float absorbed = Math.Min(currentShield, damage);
            remainingShield = currentShield - absorbed;
            return Math.Max(damage - absorbed, 0f);
        }

        /// ==================================================================
        /// DAMAGE: BaseDamage × Caster's MaxHP
        /// ==================================================================
        public static float DamageByCasterMaxHp(float baseDamage, float casterMaxHp)
        {
            return baseDamage * casterMaxHp;
        }

        /// ==================================================================
        /// DAMAGE: BaseDamage × Target's MaxHP
        /// ==================================================================
        public static float DamageByTargetMaxHp(float baseDamage, float targetMaxHp)
        {
            return baseDamage * targetMaxHp;
        }

        /// ==================================================================
        /// HP% helper:  health * percent / 100, ceil to int
        /// ==================================================================
        public static int HpPercentDamage(float maxHp, float percent)
        {
            return (int)Math.Ceiling(maxHp * percent / 100f);
        }

        /// ==================================================================
        /// HEAL
        /// ==================================================================
        public static float Heal(float casterGearPower, float casterHealAmount, float casterProficiency,
            float targetMaxHp, float targetMaxHpPercent, float intensity)
        {
            float heal = casterGearPower * casterHealAmount * casterProficiency * targetMaxHp + targetMaxHp * targetMaxHpPercent;
            return heal * intensity;
        }

        /// ==================================================================
        /// SHIELD (same formula as Heal)
        /// ==================================================================
        public static float Shield(float casterGearPower, float casterShieldAmount, float casterProficiency,
            float targetMaxHp, float targetMaxHpPercent, float intensity)
        {
            return Heal(casterGearPower, casterShieldAmount, casterProficiency, targetMaxHp, targetMaxHpPercent, intensity);
        }

        /// ==================================================================
        /// AVERAGE DAMAGE / DPS (used for gear stats / UI preview)
        /// ==================================================================
        public static float AverageDamage(float baseDamage, float casterGearPower,
            float casterAttack, float casterGearAttack,
            float casterCritRate, float casterGearCritRate,
            float casterCritDamageBonus, float casterGearCritDamageBonus,
            float skillDpsMod, List<float> buffDpsMods)
        {
            float attack = casterAttack * casterGearAttack;
            float totalCritRate = casterCritRate + casterGearCritRate;
            float totalCritDmg = casterCritDamageBonus + casterGearCritDamageBonus;
            float avgCritMul = 1f + totalCritRate * (totalCritDmg - 1f);

            float mod2Sum = 0f;
            if (buffDpsMods != null)
                for (int i = 0; i < buffDpsMods.Count; i++)
                    mod2Sum += buffDpsMods[i];

            return baseDamage * casterGearPower * attack
                 * avgCritMul
                 * skillDpsMod
                 * (1f + mod2Sum);
        }

        /// ==================================================================
        /// BLAST AoE radius (used by adapter to scale area)
        /// ==================================================================
        public static float BlastRadius(float baseRadius,
            float casterAoeSizeBonus, float casterGearAoeSizeBonus)
        {
            return baseRadius * (1f + casterAoeSizeBonus + casterGearAoeSizeBonus);
        }
    }
}
