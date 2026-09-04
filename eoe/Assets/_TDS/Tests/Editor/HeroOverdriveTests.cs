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
                id = 101,
                health = 10,
                attack = 1,
                attackRange = 1,
                attackSpeed = 1,
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
            Assert.That(hero.HeroId, Is.EqualTo(101));
            Assert.That(hero.TryStartOverdrive(5f), Is.True);
            Assert.That(hero.IsOverdriveActive, Is.True);
            Assert.That(hero.OverdriveRemaining, Is.EqualTo(5f));
            Assert.That(hero.OverdriveActivations, Is.EqualTo(1));
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
