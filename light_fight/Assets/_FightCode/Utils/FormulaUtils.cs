using System;
using System.Collections.Generic;
using _FightCode.Config;
using _KITSystem.Formula;

namespace _FightCode.Utils
{
    public static class FormulaUtils
    {
        private static readonly Dictionary<int, StatComplex> MonsterPowerStatsComplex = new Dictionary<int, StatComplex>();
        private static readonly Dictionary<int, StatComplex> MonsterStatsComplex = new Dictionary<int, StatComplex>();
        private static readonly Dictionary<int, StatComplex> WeaponStatsComplex = new Dictionary<int, StatComplex>();

        public static int MonsterPower(MonsterData monsterData, SkillData skillData, int skillLevel)
        {
            int hash = HashCode.Combine(monsterData.skillId, monsterData.skillId, monsterData.skillLevel);

            if (MonsterPowerStatsComplex.TryGetValue(hash, out StatComplex statComplex))
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
                
                MonsterPowerStatsComplex.Add(hash, statComplex);
                
                return (int)statComplex.GetPower(skillData.ScaleDamage(skillLevel), skillData.FlatDamage(skillLevel));
            }
        }

        public static int OutputDamage(WeaponData weaponData, MonsterData monsterData, SkillData skillData, int skillLevel)
        {
            StatComplex source = GetComplex(weaponData, skillData, skillLevel);
            StatComplex target = GetComplex(monsterData);
            return OutputDamage(source, target, skillData, skillLevel);
        }
        
        private static StatComplex GetComplex(MonsterData monsterData)
        {
            if (MonsterStatsComplex.TryGetValue(monsterData.monsterId, out StatComplex statComplex))
            {
                return statComplex;
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

                MonsterStatsComplex.Add(monsterData.monsterId, statComplex);
                return statComplex;
            }
        }

        private static StatComplex GetComplex(WeaponData weaponData, SkillData skillData, int skillLevel)
        {
            int hash = HashCode.Combine(weaponData.skillId, skillData.skillId, skillLevel);
            
            if (WeaponStatsComplex.TryGetValue(hash, out StatComplex statComplex))
            {
                return statComplex;
            }
            else
            {
                statComplex = new StatComplex();
                statComplex.AddBase(StatType.Attack, weaponData.Attack(skillLevel));
                statComplex.AddBase(StatType.CriticalRate, 0);
                statComplex.AddBase(StatType.CriticalDamage, 0);
                statComplex.AddBase(StatType.DamageMultiplier, 0);
                statComplex.AddBase(StatType.ArmorPenPercent, 0);

                WeaponStatsComplex.Add(hash, statComplex);
                return statComplex;
            }
        }
        
        private static int OutputDamage(StatComplex source, StatComplex target, SkillData skillData, int skillLevel)
        {
            StatSnapshot s = source.CreateSnapshot();
            StatSnapshot t = target.CreateSnapshot();
            return (int) s.DamageDealtTo(t, skillData.ScaleDamage(skillLevel), skillData.FlatDamage(skillLevel));
        }
    }
}