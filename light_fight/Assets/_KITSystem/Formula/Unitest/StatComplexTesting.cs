using NUnit.Framework;

namespace _KITSystem.Formula.Unitest
{
    public class StatComplexTesting
    {
        private const float DELTA = 0.001f;

        [Test]
        public void GetValue_StatNotCreated_ShouldReturnZero()
        {
            StatComplex stats = new StatComplex();

            Assert.AreEqual(0, stats.GetBase(StatType.Attack), DELTA);
            Assert.AreEqual(0, stats.GetValue(StatType.Attack), DELTA);
        }

        [Test]
        public void SetBaseAndAddBase_ShouldChangeBaseAndValue()
        {
            StatComplex stats = new StatComplex();

            stats.SetBase(StatType.Attack, 100);
            stats.AddBase(StatType.Attack, 25);

            Assert.AreEqual(125, stats.GetBase(StatType.Attack), DELTA);
            Assert.AreEqual(125, stats.GetValue(StatType.Attack), DELTA);
        }

        [Test]
        public void GetValue_WithAllModifierTypes_ShouldApplyExpectedFormula()
        {
            StatComplex stats = new StatComplex();
            stats.SetBase(StatType.Attack, 100);

            stats.AddModifier(new StatModifier(StatType.Attack, StatModifierType.Flat, 25));
            stats.AddModifier(new StatModifier(StatType.Attack, StatModifierType.AddPercent, 0.1f));
            stats.AddModifier(new StatModifier(StatType.Attack, StatModifierType.AddPercent, 0.2f));
            stats.AddModifier(new StatModifier(StatType.Attack, StatModifierType.MorePercent, 0.5f));
            stats.AddModifier(new StatModifier(StatType.Attack, StatModifierType.MorePercent, 0.2f));

            float expected = (100 + 25) * (1f + 0.1f + 0.2f) * 1.5f * 1.2f;
            Assert.AreEqual(expected, stats.GetValue(StatType.Attack), DELTA);
        }

        [Test]
        public void RemoveModifier_ShouldRemoveOnlyExactModifier()
        {
            StatComplex stats = new StatComplex();
            object source = new object();
            StatModifier flat = new StatModifier(StatType.Attack, StatModifierType.Flat, 10, source);
            StatModifier percent = new StatModifier(StatType.Attack, StatModifierType.AddPercent, 0.5f, source);

            stats.SetBase(StatType.Attack, 100);
            stats.AddModifier(flat);
            stats.AddModifier(percent);

            Assert.IsTrue(stats.RemoveModifier(flat));
            Assert.AreEqual(150, stats.GetValue(StatType.Attack), DELTA);
            Assert.IsFalse(stats.RemoveModifier(flat));
        }

        [Test]
        public void RemoveModifiersFrom_ShouldRemoveAllModifiersWithSameSource()
        {
            StatComplex stats = new StatComplex();
            object equipment = new object();
            object buff = new object();

            stats.SetBase(StatType.Attack, 100);
            stats.SetBase(StatType.Health, 1000);
            stats.AddModifier(new StatModifier(StatType.Attack, StatModifierType.Flat, 50, equipment));
            stats.AddModifier(new StatModifier(StatType.Health, StatModifierType.AddPercent, 0.5f, equipment));
            stats.AddModifier(new StatModifier(StatType.Attack, StatModifierType.Flat, 25, buff));

            int removed = stats.RemoveModifiersFrom(equipment);

            Assert.AreEqual(2, removed);
            Assert.AreEqual(125, stats.GetValue(StatType.Attack), DELTA);
            Assert.AreEqual(1000, stats.GetValue(StatType.Health), DELTA);
        }

        [Test]
        public void ClearModifiers_ShouldOnlyClearTargetStat()
        {
            StatComplex stats = new StatComplex();

            stats.SetBase(StatType.Attack, 100);
            stats.SetBase(StatType.Health, 1000);
            stats.AddModifier(new StatModifier(StatType.Attack, StatModifierType.Flat, 50));
            stats.AddModifier(new StatModifier(StatType.Health, StatModifierType.Flat, 500));

            stats.ClearModifiers(StatType.Attack);

            Assert.AreEqual(100, stats.GetValue(StatType.Attack), DELTA);
            Assert.AreEqual(1500, stats.GetValue(StatType.Health), DELTA);
        }

        [Test]
        public void ClearAllModifiers_ShouldClearEveryStatModifier()
        {
            StatComplex stats = new StatComplex();

            stats.SetBase(StatType.Attack, 100);
            stats.SetBase(StatType.Health, 1000);
            stats.AddModifier(new StatModifier(StatType.Attack, StatModifierType.Flat, 50));
            stats.AddModifier(new StatModifier(StatType.Health, StatModifierType.Flat, 500));

            stats.ClearAllModifiers();

            Assert.AreEqual(100, stats.GetValue(StatType.Attack), DELTA);
            Assert.AreEqual(1000, stats.GetValue(StatType.Health), DELTA);
        }

        [Test]
        public void CreateSnapshot_ShouldClampCombatRates()
        {
            StatComplex stats = new StatComplex();
            stats.SetBase(StatType.AttackSpeed, -1);
            stats.SetBase(StatType.CriticalRate, 2);
            stats.SetBase(StatType.CriticalDamage, -1);
            stats.SetBase(StatType.DamageMultiplier, -2);
            stats.SetBase(StatType.ArmorPenPercent, 2);

            StatSnapshot snapshot = stats.CreateSnapshot();

            Assert.AreEqual(0, snapshot.AttackSpeed, DELTA);
            Assert.AreEqual(1, snapshot.CriticalRate, DELTA);
            Assert.AreEqual(0, snapshot.CriticalDamage, DELTA);
            Assert.AreEqual(0, snapshot.DamageMultiplier, DELTA);
            Assert.AreEqual(1, snapshot.ArmorPenPercent, DELTA);
        }

        [Test]
        public void GetDps_ShouldUseSkillDamageAttackSpeedCriticalAndMultiplier()
        {
            StatComplex stats = new StatComplex();
            stats.SetBase(StatType.Attack, 100);
            stats.SetBase(StatType.AttackSpeed, 2);
            stats.SetBase(StatType.CriticalRate, 0.5f);
            stats.SetBase(StatType.CriticalDamage, 1);
            stats.SetBase(StatType.DamageMultiplier, 0.25f);

            float dps = stats.CreateSnapshot().Dps(1.5f, 25);

            Assert.AreEqual(656.25f, dps, DELTA);
        }

        [Test]
        public void GetEffectiveHp_ShouldApplyDefenseReduction()
        {
            StatComplex stats = new StatComplex();
            stats.SetBase(StatType.Health, 1000);
            stats.SetBase(StatType.Defense, 1000);

            Assert.AreEqual(2000, stats.CreateSnapshot().EffectiveHp(), DELTA);
        }

        [Test]
        public void GetEffectiveHp_WithArmorPen_ShouldReduceDefenseBeforeCalculation()
        {
            StatComplex stats = new StatComplex();
            stats.SetBase(StatType.Health, 1000);
            stats.SetBase(StatType.Defense, 1000);
            stats.SetBase(StatType.ArmorPenPercent, 0.5f);

            Assert.AreEqual(1500, stats.CreateSnapshot().EffectiveHp(), DELTA);
        }

        [Test]
        public void GetPower_ShouldReturnSqrtOfDpsTimesEffectiveHp()
        {
            StatComplex stats = new StatComplex();
            stats.SetBase(StatType.Attack, 100);
            stats.SetBase(StatType.AttackSpeed, 1);
            stats.SetBase(StatType.Health, 1000);
            stats.SetBase(StatType.Defense, 1000);

            Assert.AreEqual(447.2136f, stats.GetPower(), DELTA);
        }
    }
}
