using NUnit.Framework;
using UnityEngine;
using _TDS.Battle;
using _TDS.GameConfig;
using _GameToolkit.Statistics;

namespace _TDS.Tests.Editor
{
    public class HeroOverdriveIdentityTests
    {
        private GameObject gameObject;
        private Hero hero;

        [SetUp]
        public void SetUp()
        {
            gameObject = new GameObject("HeroOverdriveIdentityTests");
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
        public void DpsIdentityBoostsAttackSpeedOnlyWhileActive()
        {
            hero.SetOverdriveIdentity(OverdriveIdentity.Dps);
            hero.TryStartOverdrive(5f);

            Assert.That(hero.GetStat(StatId.AttackSpeed).Value, Is.EqualTo(1.5f));
            hero.TickOverdrive(5f);
            Assert.That(hero.GetStat(StatId.AttackSpeed).Value, Is.EqualTo(1f));
        }

        [Test]
        public void AreaIdentityMarksMultiTargetAttackWindow()
        {
            hero.SetOverdriveIdentity(OverdriveIdentity.Aoe);
            hero.TryStartOverdrive(5f);

            Assert.That(hero.IsAreaOverdriveActive, Is.True);
            hero.TickOverdrive(5f);
            Assert.That(hero.IsAreaOverdriveActive, Is.False);
        }

        [Test]
        public void DefenseIdentityAbsorbsDamageWithTemporaryShield()
        {
            hero.SetOverdriveIdentity(OverdriveIdentity.Defense);
            hero.TryStartOverdrive(5f);

            Assert.That(hero.OverdriveShield, Is.EqualTo(3));
            hero.TakeDamage(2);
            Assert.That(hero.CurrentHealth, Is.EqualTo(10));
            Assert.That(hero.OverdriveShield, Is.EqualTo(1));
            hero.TakeDamage(2);
            Assert.That(hero.CurrentHealth, Is.EqualTo(9));
            Assert.That(hero.OverdriveShield, Is.EqualTo(0));
        }
    }
}
