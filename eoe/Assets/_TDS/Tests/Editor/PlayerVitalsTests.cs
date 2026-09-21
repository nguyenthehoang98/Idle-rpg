using NUnit.Framework;
using UnityEngine;
using _TDS.Battle;
using _TDS.GameConfig;

namespace _TDS.Tests.Editor
{
    /// <summary>
    /// HP là pool chung của player, không còn nằm trên từng hero.
    /// </summary>
    public class PlayerVitalsTests
    {
        private GameObject first;
        private GameObject second;

        private static Hero CreateHero(GameObject host, int id, int health)
        {
            Hero hero = host.AddComponent<Hero>();
            hero.Initialize(new HeroConfigData
            {
                id = id,
                health = health,
                attack = 1,
                attackRange = 1,
                attackSpeed = 1,
                attackId = 1101,
            }, new SkillConfigData());
            return hero;
        }

        [SetUp]
        public void SetUp()
        {
            PlayerVitals.Reset();
            first = new GameObject("HeroA");
            second = new GameObject("HeroB");
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(second);
            Object.DestroyImmediate(first);
            PlayerVitals.Reset();
        }

        [Test]
        public void PoolIsSumOfAliveHeroMaxHealth()
        {
            CreateHero(first, 1001, 10);
            Assert.That(PlayerVitals.MaxHealth, Is.EqualTo(10));
            Assert.That(PlayerVitals.CurrentHealth, Is.EqualTo(10));

            CreateHero(second, 1002, 5);
            Assert.That(PlayerVitals.MaxHealth, Is.EqualTo(15));
            Assert.That(PlayerVitals.CurrentHealth, Is.EqualTo(15));
        }

        [Test]
        public void DamageHitsSharedPoolNotTheHero()
        {
            Hero hero = CreateHero(first, 1001, 10);

            Assert.That(PlayerVitals.TakeDamage(4), Is.EqualTo(4));
            Assert.That(PlayerVitals.CurrentHealth, Is.EqualTo(6));
            Assert.That(hero.MaxHealth, Is.EqualTo(10), "Hero không có HP riêng để bị trừ");
            Assert.That(hero.IsDead, Is.False, "Hero vẫn sống khi pool còn máu");
        }

        [Test]
        public void PoolEmptyFiresDeathOnceAndClamps()
        {
            CreateHero(first, 1001, 10);
            int deaths = 0;
            PlayerVitals.OnDied += () => deaths++;

            Assert.That(PlayerVitals.TakeDamage(999), Is.EqualTo(10), "Chỉ trừ đúng phần máu còn lại");
            Assert.That(PlayerVitals.CurrentHealth, Is.EqualTo(0));
            Assert.That(deaths, Is.EqualTo(1));

            Assert.That(PlayerVitals.TakeDamage(5), Is.EqualTo(0), "Chết rồi thì không nhận thêm sát thương");
            Assert.That(deaths, Is.EqualTo(1));
        }

        [Test]
        public void HealDoesNotExceedPoolAndIsIgnoredAfterDeath()
        {
            CreateHero(first, 1001, 10);
            PlayerVitals.TakeDamage(6);

            PlayerVitals.Heal(100);
            Assert.That(PlayerVitals.CurrentHealth, Is.EqualTo(10));

            PlayerVitals.TakeDamage(10);
            PlayerVitals.Heal(5);
            Assert.That(PlayerVitals.CurrentHealth, Is.EqualTo(0));
        }
    }
}
