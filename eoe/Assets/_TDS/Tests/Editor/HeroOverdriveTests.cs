using NUnit.Framework;
using UnityEngine;
using _TDS.Battle;
using _TDS.GameConfig;

namespace _TDS.Tests.Editor
{
    public class HeroOverdriveTests
    {
        private GameObject gameObject;
        private Hero hero;

        [SetUp]
        public void SetUp()
        {
            gameObject = new GameObject("HeroOverdriveTests");
            hero = gameObject.AddComponent<Hero>();
            hero.Initialize(new HeroConfigData
            {
                id = 1001,
                health = 10,
                attack = 1,
                attackRange = 1,
                attackSpeed = 1,
                attackId = 1101,
            }, new SkillConfigData());
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(gameObject);
        }

        [Test]
        public void OverdriveStartsWithExplicitState()
        {
            Assert.That(hero.HeroId, Is.EqualTo(1001));
            Assert.That(hero.SkillId, Is.EqualTo(1101));
            Assert.That(hero.TryStartOverdrive(5f), Is.True);
            Assert.That(hero.IsOverdriveActive, Is.True);
            Assert.That(hero.OverdriveRemaining, Is.EqualTo(5f));
            Assert.That(hero.OverdriveActivations, Is.EqualTo(1));
        }

        [Test]
        public void OverdriveAppliesConfiguredStatUpgradeUntilDurationEnds()
        {
            hero.Initialize(new HeroConfigData
            {
                id = 1001,
                health = 10,
                attack = 1,
                attackRange = 1,
                attackSpeed = 1,
                attackId = 1101,
            }, new SkillConfigData());
            hero.SetPowerUpgrades(new[]
            {
                new StatUpgradeConfigData
                {
                    id = 1001,
                    stat = StatId.Attack,
                    value = 2f,
                },
                new StatUpgradeConfigData
                {
                    id = 1002,
                    stat = StatId.AttackSpeed,
                    value = 0.5f,
                    percent = true,
                },
            });

            Assert.That(hero.GetStat(StatId.Attack).Value, Is.EqualTo(1f));
            Assert.That(hero.GetStat(StatId.AttackSpeed).Value, Is.EqualTo(1f));
            Assert.That(hero.TryStartOverdrive(5f), Is.True);
            Assert.That(hero.GetStat(StatId.Attack).Value, Is.EqualTo(3f));
            Assert.That(hero.GetStat(StatId.AttackSpeed).Value, Is.EqualTo(1.5f));

            hero.TickOverdrive(5f);

            Assert.That(hero.IsOverdriveActive, Is.False);
            Assert.That(hero.GetStat(StatId.Attack).Value, Is.EqualTo(1f));
            Assert.That(hero.GetStat(StatId.AttackSpeed).Value, Is.EqualTo(1f));
        }

        [Test]
        public void OverdriveDoesNotOverlapAndEndsAfterDuration()
        {
            hero.TryStartOverdrive(5f);

            Assert.That(hero.TryStartOverdrive(5f), Is.False);
            hero.TickOverdrive(4.9f);
            Assert.That(hero.IsOverdriveActive, Is.True);
            hero.TickOverdrive(0.1f);

            Assert.That(hero.IsOverdriveActive, Is.False);
            Assert.That(hero.OverdriveRemaining, Is.EqualTo(0f));
        }
    }
}
