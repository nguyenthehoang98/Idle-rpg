#if UNITY_INCLUDE_TESTS
using System;
using System.Collections;
using _KITSystem.Config;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.TestTools;

namespace _Game.Configs
{
    public class GameConfigTest
    {
        [UnityTest]
        public IEnumerator LoadAddressable_MonsterConfig()
        {
            yield return LoadBundle<MonsterConfig>();
        }

        [UnityTest]
        public IEnumerator LoadAddressable_LevelConfig()
        {
            yield return LoadBundle<LevelConfig>();
        }

        [UnityTest]
        public IEnumerator LoadAddressable_WeaponConfig()
        {
            yield return LoadBundle<WeaponConfig>();
        }

        [UnityTest]
        public IEnumerator LoadAddressable_PlayerConfig()
        {
            yield return LoadBundle<PlayerConfig>();
        }

        private IEnumerator LoadBundle<T>(Action<T> callback = null) where T : IGameConfig
        {
            string name = typeof(T).Name;

            AsyncOperationHandle<TextAsset> handle = Addressables.LoadAssetAsync<TextAsset>(name);

            yield return handle;

            Assert.AreEqual(AsyncOperationStatus.Succeeded, handle.Status);

            Assert.NotNull(handle.Result);

            try
            {
                T data = JsonUtility.FromJson<T>(handle.Result.text);

                data.OnMappingValue();

                callback?.Invoke(data);
            }
            catch (Exception e)
            {
                Assert.Fail($"Parse error game_config '{name}'");
            }

            Addressables.Release(handle);
        }

        [Test]
        public void Increase_Should_Add_All_Values()
        {
            var current = new WeaponUpgradeData();

            var bonus = new WeaponUpgradeData
            {
                attackSpeed = 1,
                projectileSize = 2,
                damagePercent = 3,
                cooldownReduce = 4,
                parallelCount = 5,
                spreadCount = 6,
                spreadDamagePercent = 7,
                piercingCount = 8,
                explosiveRadius = 9,
                explosiveDamagePercent = 10,
                critChance = 11,
                critDamage = 12,
                bounceCount = 13,
                bounceDamagePercent = 14,
                killInstantBelowHealthPercent = 15,
            };

            current.Increase(bonus);

            Assert.AreEqual(1, current.attackSpeed);
            Assert.AreEqual(2, current.projectileSize);
            Assert.AreEqual(3, current.damagePercent);
            Assert.AreEqual(4, current.cooldownReduce);
            Assert.AreEqual(5, current.parallelCount);
            Assert.AreEqual(6, current.spreadCount);
            Assert.AreEqual(7, current.spreadDamagePercent);
            Assert.AreEqual(8, current.piercingCount);
            Assert.AreEqual(9, current.explosiveRadius);
            Assert.AreEqual(10, current.explosiveDamagePercent);
            Assert.AreEqual(11, current.critChance);
            Assert.AreEqual(12, current.critDamage);
            Assert.AreEqual(13, current.bounceCount);
            Assert.AreEqual(14, current.bounceDamagePercent);
            Assert.AreEqual(15, current.killInstantBelowHealthPercent);
        }

        [Test]
        public void Increase_Then_Decrease_Should_Return_To_Original()
        {
            var original = new WeaponUpgradeData
            {
                attackSpeed = 10,
                projectileSize = 20,
                damagePercent = 30,
                cooldownReduce = 40,
                parallelCount = 50,
                spreadCount = 60,
                spreadDamagePercent = 70,
                piercingCount = 80,
                explosiveRadius = 90,
                explosiveDamagePercent = 100,
                critChance = 110,
                critDamage = 120,
                bounceCount = 130,
                bounceDamagePercent = 140,
                killInstantBelowHealthPercent = 150
            };

            var current = original;

            var bonus = new WeaponUpgradeData
            {
                attackSpeed = 1,
                projectileSize = 2,
                damagePercent = 3,
                cooldownReduce = 4,
                parallelCount = 5,
                spreadCount = 6,
                spreadDamagePercent = 7,
                piercingCount = 8,
                explosiveRadius = 9,
                explosiveDamagePercent = 10,
                critChance = 11,
                critDamage = 12,
                bounceCount = 13,
                bounceDamagePercent = 14,
                killInstantBelowHealthPercent = 15
            };

            current.Increase(bonus);
            current.Decrease(bonus);

            Assert.AreEqual(original.attackSpeed, current.attackSpeed);
            Assert.AreEqual(original.projectileSize, current.projectileSize);
            Assert.AreEqual(original.damagePercent, current.damagePercent);
            Assert.AreEqual(original.cooldownReduce, current.cooldownReduce);
            Assert.AreEqual(original.parallelCount, current.parallelCount);
            Assert.AreEqual(original.spreadCount, current.spreadCount);
            Assert.AreEqual(original.spreadDamagePercent, current.spreadDamagePercent);
            Assert.AreEqual(original.piercingCount, current.piercingCount);
            Assert.AreEqual(original.explosiveRadius, current.explosiveRadius);
            Assert.AreEqual(original.explosiveDamagePercent, current.explosiveDamagePercent);
            Assert.AreEqual(original.critChance, current.critChance);
            Assert.AreEqual(original.critDamage, current.critDamage);
            Assert.AreEqual(original.bounceCount, current.bounceCount);
            Assert.AreEqual(original.bounceDamagePercent, current.bounceDamagePercent);
            Assert.AreEqual(original.killInstantBelowHealthPercent, current.killInstantBelowHealthPercent);
        }
    }
}
#endif