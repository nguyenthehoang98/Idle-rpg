using NUnit.Framework;
using _GameToolkit.Avoidance;
using _TDS.Battle;
using _TDS.GameConfig;
using UnityEngine;

namespace _TDS.Tests.Editor
{
    public class CombatDamageTests
    {
        [Test]
        public void SetAgentMaxSpeedCompletesPreviousSimulationStep()
        {
            AgentSimulator simulator = new AgentSimulator();
            simulator.Initialize();

            try
            {
                int agent = simulator.CreateAgent(
                    new Vector2(10f, 0f), 1f, 1f, 0.1f).agent;
                simulator.Tick(0.1f);

                Assert.DoesNotThrow(() => simulator.SetAgentMaxSpeed(agent, 0.5f));
            }
            finally
            {
                simulator.Dispose();
            }
        }

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

        [Test]
        public void ModifierSelectionUsesWeightThenSuccessRate()
        {
            SkillModifierData[] modifiers =
            {
                new SkillModifierData { type = SkillModifierType.Slow, weight = 1f, successRate = 1f },
                new SkillModifierData { type = SkillModifierType.Stun, weight = 3f, successRate = 0.5f },
            };

            Assert.That(SkillModifierSelector.TrySelect(modifiers, 0.9f, 0.2f, out SkillModifierData selected), Is.True);
            Assert.That(selected.type, Is.EqualTo(SkillModifierType.Stun));
            Assert.That(SkillModifierSelector.TrySelect(modifiers, 0.9f, 0.9f, out _), Is.False);
            Assert.That(SkillModifierSelector.TrySelect(
                new[] { new SkillModifierData { type = SkillModifierType.Slow, successRate = 1f } },
                0f, 0f, out _), Is.False);
        }
    }
}
