#if UNITY_INCLUDE_TESTS
using _GameToolkit.Statistics;
using NUnit.Framework;

namespace _GameToolkit.Tests
{
    public class StatTests
    {
        [Test]
        public void Value_NoModifiers_EqualsBase()
        {
            var stat = new Stat(100f);

            Assert.AreEqual(100f, stat.Value);
        }

        [Test]
        public void Value_FlatModifier_AddsToBase()
        {
            var stat = new Stat(100f);

            stat.AddModifier(new StatModifier(25f, StatModType.Flat));

            Assert.AreEqual(125f, stat.Value);
        }

        [Test]
        public void Value_MultipleFlat_AllAdd()
        {
            var stat = new Stat(100f);

            stat.AddModifier(new StatModifier(10f, StatModType.Flat));
            stat.AddModifier(new StatModifier(5f, StatModType.Flat));

            Assert.AreEqual(115f, stat.Value);
        }

        [Test]
        public void Value_PercentAdd_AfterFlat()
        {
            var stat = new Stat(100f);

            stat.AddModifier(new StatModifier(50f, StatModType.PercentAdd));
            stat.AddModifier(new StatModifier(10f, StatModType.Flat));

            // (100 + 10) * (1 + 0.5) = 165
            Assert.AreEqual(165f, stat.Value);
        }

        [Test]
        public void Value_PercentAdd_SumsBeforeMultiply()
        {
            var stat = new Stat(100f);

            stat.AddModifier(new StatModifier(0.1f, StatModType.PercentAdd));
            stat.AddModifier(new StatModifier(0.2f, StatModType.PercentAdd));

            // 100 * (1 + 0.3) = 130
            Assert.AreEqual(130f, stat.Value);
        }

        [Test]
        public void Value_PercentMult_MultipliesIndividually()
        {
            var stat = new Stat(100f);

            stat.AddModifier(new StatModifier(0.1f, StatModType.PercentMult));
            stat.AddModifier(new StatModifier(0.1f, StatModType.PercentMult));

            // 100 * 1.1 * 1.1 = 121
            Assert.AreEqual(121f, stat.Value);
        }

        [Test]
        public void Value_ModifierOrder_FlatBeforePercent()
        {
            var stat = new Stat(100f);

            // order: PercentAdd(200) > Flat(100) → flat trước dù add sau
            stat.AddModifier(new StatModifier(0.5f, StatModType.PercentAdd));
            stat.AddModifier(new StatModifier(10f, StatModType.Flat));

            // (100 + 10) * 1.5 = 165
            Assert.AreEqual(165f, stat.Value);
        }

        [Test]
        public void RemoveModifier_Recalculates()
        {
            var stat = new Stat(100f);
            var mod = new StatModifier(50f, StatModType.Flat);

            stat.AddModifier(mod);
            Assert.AreEqual(150f, stat.Value);

            Assert.IsTrue(stat.RemoveModifier(mod));
            Assert.AreEqual(100f, stat.Value);
        }

        [Test]
        public void RemoveAllModifiersFromSource_OnlyRemovesThatSource()
        {
            var stat = new Stat(100f);
            var sourceA = new object();
            var sourceB = new object();

            stat.AddModifier(new StatModifier(10f, StatModType.Flat, sourceA));
            stat.AddModifier(new StatModifier(20f, StatModType.Flat, sourceB));

            Assert.IsTrue(stat.RemoveAllModifiersFromSource(sourceA));
            Assert.AreEqual(120f, stat.Value);

            // Remove lại sourceA → không còn gì
            Assert.IsFalse(stat.RemoveAllModifiersFromSource(sourceA));
        }

        [Test]
        public void BaseValueChanged_ValueRecalculates()
        {
            var stat = new Stat(100f);
            stat.AddModifier(new StatModifier(0.5f, StatModType.PercentAdd));

            Assert.AreEqual(150f, stat.Value);

            stat.BaseValue = 200f;

            Assert.AreEqual(300f, stat.Value);
        }
    }
}
#endif
