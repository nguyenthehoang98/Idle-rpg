using NUnit.Framework;
using UnityEngine;
using _GameToolkit.Skills;
using _TDS.Battle;
using _TDS.GameConfig;

namespace _TDS.Tests.Editor
{
    /// <summary>
    /// Monster vừa spawn còn chạy animation Init: chưa di chuyển, chưa nhận sát thương/modifier.
    /// </summary>
    public class MonsterSpawnDelayTests
    {
        private GameObject gameObject;
        private Monster monster;

        [SetUp]
        public void SetUp()
        {
            gameObject = new GameObject("MonsterSpawnDelayTests");
            gameObject.AddComponent<MonsterSortingLayer>();
            monster = gameObject.AddComponent<Monster>();
            monster.SetCombatData(maxHealth: 10, attack: 1, attackRange: 1f, damageCooldown: 1f);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(gameObject);
        }

        private void Spawn(float delay)
        {
            monster.SpawnDelay = delay;
            monster.BeginSpawnDelay();
        }

        [Test]
        public void BeginSpawnDelayPutsMonsterInSpawningState()
        {
            Spawn(0.5f);

            Assert.That(monster.IsSpawning, Is.True);
        }

        [Test]
        public void SpawnDelayEndsAfterConfiguredDuration()
        {
            Spawn(0.5f);

            monster.TickSpawnDelay(0.49f);
            Assert.That(monster.IsSpawning, Is.True);

            monster.TickSpawnDelay(0.02f);
            Assert.That(monster.IsSpawning, Is.False);
        }

        [Test]
        public void ZeroDelayMeansMonsterIsReadyImmediately()
        {
            Spawn(0f);

            Assert.That(monster.IsSpawning, Is.False);
            Assert.That(monster.TakeDamage(4), Is.EqualTo(4));
        }

        [Test]
        public void TakesNoDamageWhileSpawning()
        {
            Spawn(0.5f);

            Assert.That(monster.TakeDamage(4), Is.EqualTo(0));
            Assert.That(monster.CurrentHealth, Is.EqualTo(10));

            monster.TickSpawnDelay(0.5f);
            Assert.That(monster.TakeDamage(4), Is.EqualTo(4));
            Assert.That(monster.CurrentHealth, Is.EqualTo(6));
        }

        [Test]
        public void CannotBeKilledWhileSpawning()
        {
            Spawn(0.5f);

            Assert.That(monster.TakeDamage(999), Is.EqualTo(0));
            Assert.That(monster.CurrentHealth, Is.EqualTo(10), "Đang spawn thì không được chết");

            monster.TickSpawnDelay(0.5f);
            Assert.That(monster.TakeDamage(999), Is.EqualTo(10), "Hết delay mới nhận sát thương");
        }

        [Test]
        public void ModifierIsRejectedWhileSpawning()
        {
            Spawn(0.5f);
            var modifier = new SkillModifierData
            {
                type = SkillModifierType.Stun,
                duration = 1f,
                value = 1f,
            };
            var action = new ModifierSkillAction(1f, null, null, null);

            Assert.That(monster.TryApplyModifier(action, modifier, 1f), Is.False, "Đang spawn thì không nhận stun");
            Assert.That(monster.IsStunned, Is.False);

            monster.TickSpawnDelay(0.5f);
            Assert.That(monster.TryApplyModifier(action, modifier, 1f), Is.True);
            Assert.That(monster.IsStunned, Is.True);
        }
    }
}
