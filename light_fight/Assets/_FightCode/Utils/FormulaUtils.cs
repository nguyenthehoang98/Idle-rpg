using System;
using System.Collections.Generic;
using _FightCode.Config;
using _KITSystem.Formula;

namespace _FightCode.Utils
{
    public static class FormulaUtils
    {
        private static Dictionary<int, StatComplex> monsterStatsComplex = new Dictionary<int, StatComplex>();

        public static int MonsterPower(MonsterData monsterData, SkillData skillData)
        {
            int hash = HashCode.Combine(monsterData.SkillId, monsterData.SkillId, monsterData.SkillLevel);

            if (monsterStatsComplex.TryGetValue(hash, out StatComplex statComplex))
            {
                return (int)statComplex.GetPower(skillData.ScaleDamage(), skillData.FlatDamage());
            }
            else
            {
                statComplex = new StatComplex();
                statComplex.AddBase(StatType.Attack, monsterData.Attack);
                statComplex.AddBase(StatType.Health, monsterData.Health);
                statComplex.AddBase(StatType.Defense, monsterData.Defense);
                statComplex.AddBase(StatType.AttackSpeed, monsterData.AttackSpeed);
                statComplex.AddBase(StatType.CriticalRate, monsterData.CriticalRate);
                statComplex.AddBase(StatType.CriticalDamage, monsterData.CriticalDamage);
                statComplex.AddBase(StatType.DamageMultiplier, monsterData.DamageMultiplier);
                statComplex.AddBase(StatType.ArmorPenPercent, monsterData.ArmorPenPercent);
                
                monsterStatsComplex.Add(hash, statComplex);
                return (int)statComplex.GetPower(skillData.ScaleDamage(), skillData.FlatDamage());
            }
        }
    }
}