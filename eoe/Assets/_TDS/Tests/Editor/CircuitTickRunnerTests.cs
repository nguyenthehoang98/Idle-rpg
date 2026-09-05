using NUnit.Framework;
using UnityEngine;
using _GameToolkit.Statistics;
using _TDS.Battle;
using _TDS.GameConfig;
using _TDS.Gameplay;

namespace _TDS.Tests.Editor
{
    public class CircuitTickRunnerTests
    {
        private GameObject gameObject;
        private CircuitTickRunner runner;

        [SetUp]
        public void SetUp()
        {
            gameObject = new GameObject("CircuitTickRunnerTests");
            runner = gameObject.AddComponent<CircuitTickRunner>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(gameObject);
        }

        [Test]
        public void RunnerOwnsOneCircuitAndEmitsActivationOnce()
        {
            CircuitBoard board = CircuitBoard.FromHeroes(new[] { 101 });
            int activationCount = 0;
            runner.OnActivation += _ => activationCount++;

            runner.Initialize(board);
            runner.Tick(4.01f);
            runner.Tick(4.01f);
            runner.Tick(4.01f);

            Assert.That(runner.TickCount, Is.EqualTo(3));
            Assert.That(runner.Circuit, Is.Not.Null);
            Assert.That(activationCount, Is.EqualTo(1));
        }

        [Test]
        public void ResetRestartsCircuitAndTickCount()
        {
            runner.Initialize(CircuitBoard.FromHeroes(new[] { 101 }));
            runner.Tick(0.5f);
            runner.ResetCircuit();

            Assert.That(runner.TickCount, Is.EqualTo(0));
            Assert.That(runner.Circuit.PulseIndex, Is.EqualTo(0));
            Assert.That(runner.Circuit.GetSlot(0).Stack, Is.EqualTo(0));
        }

        [Test]
        public void ItemPulseActivatesHeroOverdriveThroughRunner()
        {
            GameObject heroObject = new GameObject("CircuitCombatHero");
            Hero hero = heroObject.AddComponent<Hero>();
            hero.Initialize(new HeroConfigData
            {
                id = 101,
                health = 10,
                attack = 1,
                attackRange = 1,
                attackSpeed = 1,
            }, new SkillConfigData());
            hero.SetOverdriveIdentity(OverdriveIdentity.Dps);

            CircuitBoard board = new CircuitBoard();
            board.SetItem(0, 201, CircuitItemType.Generator);
            board.SetContent(1, CircuitSlotContent.Hero(101));
            runner.OnActivation += activation =>
            {
                if (activation.Content.Type == CircuitSlotContentType.Hero && activation.Content.Id == hero.HeroId)
                {
                    hero.TryStartOverdrive(EnergyCircuit.DefaultOverdriveDuration);
                }
            };

            runner.Initialize(board);
            runner.Tick(4.5f);

            Assert.That(runner.Circuit.GetSlot(0).Content.Type, Is.EqualTo(CircuitSlotContentType.Item));
            Assert.That(runner.Circuit.GetSlot(1).IsActive, Is.True);
            Assert.That(hero.IsOverdriveActive, Is.True);
            Assert.That(hero.GetStat(StatId.AttackSpeed).Value, Is.EqualTo(1.5f));

            Assert.That(hero.OverdriveActivations, Is.EqualTo(1));

            Object.DestroyImmediate(heroObject);
        }
    }
}
