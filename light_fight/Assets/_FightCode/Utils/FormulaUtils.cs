using System;
using System.Collections.Generic;
using _FightCode.Config;
using _KITSystem.Formula;

namespace _FightCode.Utils
{
    public static class FormulaUtils
    {
        private static Dictionary<int, StatComplex> monsterStatsComplex = new Dictionary<int, StatComplex>();

        public static int MonsterPower(MonsterData monsterData, SkillData skillData, int skillLevel)
        {
            int hash = HashCode.Combine(monsterData.skillId, monsterData.skillId, monsterData.skillLevel);

            if (monsterStatsComplex.TryGetValue(hash, out StatComplex statComplex))
            {
                return (int)statComplex.GetPower(skillData.ScaleDamage(skillLevel), skillData.FlatDamage(skillLevel));
            }
            else
            {
                statComplex = new StatComplex();
                statComplex.AddBase(StatType.Attack, monsterData.attack);
                statComplex.AddBase(StatType.Health, monsterData.health);
                statComplex.AddBase(StatType.Defense, monsterData.defense);
                statComplex.AddBase(StatType.AttackSpeed, monsterData.attackSpeed);
                statComplex.AddBase(StatType.CriticalRate, monsterData.criticalRate);
                statComplex.AddBase(StatType.CriticalDamage, monsterData.criticalDamage);
                statComplex.AddBase(StatType.DamageMultiplier, monsterData.damageMultiplier);
                statComplex.AddBase(StatType.ArmorPenPercent, monsterData.armorPenPercent);
                
                monsterStatsComplex.Add(hash, statComplex);
                return (int)statComplex.GetPower(skillData.ScaleDamage(skillLevel), skillData.FlatDamage(skillLevel));
            }
        }
    }
}