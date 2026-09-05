using NUnit.Framework;
using _GameToolkit.Avoidance;
using _GameToolkit.Skills;
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

        [Test]
        public void MonsterSkillCycleSupportsEliteAndBossRanks()
        {
            GameObject gameObject = new GameObject("MonsterRankTests");
            Monster monster = gameObject.AddComponent<Monster>();
            monster.SetCombatData(
                10,
                2,
                rank: MonsterRank.Elite,
                skills: new[]
                {
                    new MonsterSkillConfigData { skillId = 2002, cooldown = 4f, damageMultiplier = 1.5f },
                });

            Assert.That(monster.Rank, Is.EqualTo(MonsterRank.Elite));
            Assert.That(monster.TryGetReadySkill(0f, out MonsterSkillConfigData eliteSkill), Is.True);
            Assert.That(eliteSkill.skillId, Is.EqualTo(2002));
            monster.CommitSkill(eliteSkill);
            Assert.That(monster.TryGetReadySkill(0f, out _), Is.False);
            Assert.That(monster.TryGetReadySkill(4f, out _), Is.True);

            monster.SetCombatData(
                10,
                2,
                rank: MonsterRank.Boss,
                skills: new[]
                {
                    new MonsterSkillConfigData { skillId = 2005, cooldown = 1f, damageMultiplier = 2f },
                    new MonsterSkillConfigData { skillId = 2006, cooldown = 1f, damageMultiplier = 1.25f },
                });

            Assert.That(monster.TryGetReadySkill(0f, out MonsterSkillConfigData firstBossSkill), Is.True);
            monster.CommitSkill(firstBossSkill);
            Assert.That(monster.TryGetReadySkill(1f, out MonsterSkillConfigData secondBossSkill), Is.True);
            Assert.That(secondBossSkill.skillId, Is.EqualTo(2006));
            Object.DestroyImmediate(gameObject);
        }

        [Test]
        public void MonsterReportsModifierApplyAndRemove()
        {
            GameObject gameObject = new GameObject("ModifierFeedbackMonster");
            GameObject rendererObject = new GameObject("Renderer");
            rendererObject.transform.SetParent(gameObject.transform);
            SpriteRenderer renderer = rendererObject.AddComponent<SpriteRenderer>();
            Monster monster = gameObject.AddComponent<Monster>();
            ModifierSkillAction action = new ModifierSkillAction(1f, null, null, null);
            SkillModifierType applied = SkillModifierType.Slow;
            SkillModifierType removed = SkillModifierType.Slow;
            monster.OnModifierApplied += type => applied = type;
            monster.OnModifierRemoved += type => removed = type;

            monster.ApplyModifier(action, new SkillModifierData { type = SkillModifierType.Stun });
            monster.RemoveModifier(action);

            Assert.That(applied, Is.EqualTo(SkillModifierType.Stun));
            Assert.That(removed, Is.EqualTo(SkillModifierType.Stun));
            Assert.That(renderer.color.r, Is.EqualTo(1f));
            Assert.That(renderer.color.g, Is.EqualTo(1f));
            monster.ApplyModifier(action, new SkillModifierData { type = SkillModifierType.Slow });
            Assert.That(renderer.color.g, Is.LessThan(1f));
            monster.RemoveModifier(action);
            Assert.That(renderer.color, Is.EqualTo(Color.white));
            Object.DestroyImmediate(gameObject);
        }
    }
}
