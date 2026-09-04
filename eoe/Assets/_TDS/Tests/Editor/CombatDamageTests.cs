using NUnit.Framework;
using _TDS.Battle;

namespace _TDS.Tests.Editor
{
    public class CombatDamageTests
    {
        [Test]
        public void CritDamageOneDoublesBaseDamage()
        {
            CombatDamage.DamageResult result = CombatDamage.Calculate(1f, 5f, 1f, 1f, 0.5f);

            Assert.That(result.IsCritical, Is.True);
            Assert.That(result.Amount, Is.EqualTo(10));
        }

        [Test]
        public void ZeroCritChanceKeepsNormalDamage()
        {
            CombatDamage.DamageResult result = CombatDamage.Calculate(1f, 5f, 0f, 1f, 0f);

            Assert.That(result.IsCritical, Is.False);
            Assert.That(result.Amount, Is.EqualTo(5));
        }

        [Test]
        public void LifestealReturnsClampedIntegerHealing()
        {
            Assert.That(CombatDamage.CalculateLifeSteal(10, 0.2f), Is.EqualTo(2));
            Assert.That(CombatDamage.CalculateLifeSteal(10, 2f), Is.EqualTo(10));
        }
    }
}
