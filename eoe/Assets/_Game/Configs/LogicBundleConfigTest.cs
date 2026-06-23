#if UNITY_INCLUDE_TESTS && UNITY_EDITOR
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace _Game.Configs
{
    public class LogicBundleConfigTest
    {
        [Test]
        public void Increase_WithZero_NoChange()
        {
            var current = new WeaponUpgradeData { damagePercent = 50 };
            var zero = new WeaponUpgradeData();
            current.Increase(zero);
            Assert.AreEqual(50f, current.damagePercent);
        }

        [Test]
        public void Decrease_WithZero_NoChange()
        {
            var current = new WeaponUpgradeData { damagePercent = 50 };
            var zero = new WeaponUpgradeData();
            current.Decrease(zero);
            Assert.AreEqual(50f, current.damagePercent);
        }

        [Test]
        public void Increase_DoesNotAffectUnrelatedFields()
        {
            var current = new WeaponUpgradeData { damagePercent = 10, cooldownReduce = 5 };
            var bonus = new WeaponUpgradeData { damagePercent = 20 };
            current.Increase(bonus);
            Assert.AreEqual(30f, current.damagePercent);
            Assert.AreEqual(5f, current.cooldownReduce);
        }

        [Test]
        public void Increase_ThenDecrease_RoundTrip()
        {
            var original = new WeaponUpgradeData { damagePercent = 100, cooldownReduce = 50 };
            var current = original;
            var delta = new WeaponUpgradeData { damagePercent = 30, cooldownReduce = 10 };

            current.Increase(delta);
            current.Decrease(delta);

            Assert.AreEqual(original.damagePercent, current.damagePercent);
            Assert.AreEqual(original.cooldownReduce, current.cooldownReduce);
        }
        
        [Test]
        public void OnMapValue_BuildsCache()
        {
            var setting = ScriptableObject.CreateInstance<ColorSetting>();
            var datas = new[]
            {
                new ColorData { id = 1, activeColor = Color.red, inactiveColor = Color.gray },
                new ColorData { id = 2, activeColor = Color.blue, inactiveColor = Color.white },
            };
            ConfigTestHelper.SetField(setting, "datas", datas);

            var mapField = setting.GetType().GetMethod("OnMapValue",
                BindingFlags.Instance | BindingFlags.NonPublic);
            mapField.Invoke(setting, null);

            Assert.IsTrue(setting.TryGetColor(1, out var c1));
            Assert.AreEqual(Color.red, c1.activeColor);
            Assert.IsTrue(setting.TryGetColor(2, out var c2));
            Assert.AreEqual(Color.blue, c2.activeColor);
        }
        
        [Test]
        public void OnMappingValue_BuildsLevelCache()
        {
            var config = new LevelConfig();
            var levels = new List<LevelData>
            {
                new LevelData { levelId = 1, levelName = "Forest" },
                new LevelData { levelId = 2, levelName = "Cave" },
            };
            ConfigTestHelper.SetField(config, "levels", levels);
            config.OnMappingValue();

            Assert.IsTrue(config.TryGetLevelData(1, out var l1));
            Assert.AreEqual("Forest", l1.levelName);
            Assert.IsTrue(config.TryGetLevelData(2, out var l2));
            Assert.AreEqual("Cave", l2.levelName);
        }

        [Test]
        public void OnPostImported_ResolvesWaveToSpawn()
        {
            var config = new LevelConfig();
            var spawns = new List<SpawnData>
            {
                new SpawnData { spawnGroupId = "sg1", monsterId = 1, totalMonster = 5 },
            };
            var waves = new List<WaveData>
            {
                new WaveData { waveId = "w1", spawnGroupId = new[] { "sg1" } },
            };
            var levels = new List<LevelData>
            {
                new LevelData { levelId = 1, wavesId = new[] { "w1" } },
            };
            ConfigTestHelper.SetField(config, "spawns", spawns);
            ConfigTestHelper.SetField(config, "waves", waves);
            ConfigTestHelper.SetField(config, "levels", levels);

            config.OnMappingValue();
            config.OnPostImported();
            config.OnMappingValue();
            config.TryGetLevelData(1, out var level);

            Assert.AreEqual(1, level.waves.Length);
            Assert.AreEqual("w1", level.waves[0].waveId);
            Assert.AreEqual(1, level.waves[0].spawns.Length);
            Assert.AreEqual("sg1", level.waves[0].spawns[0].spawnGroupId);
            Assert.AreEqual(1, level.waves[0].spawns[0].monsterId);
        }

        [Test]
        public void TryGetLevelData_MissingId_ReturnsFalse()
        {
            var config = new LevelConfig();
            config.OnMappingValue();
            Assert.IsFalse(config.TryGetLevelData(99, out _));
        }
        
        [Test]
        public void OnMappingValue_BuildsWeaponCache()
        {
            var config = new WeaponConfig();
            var weapons = new List<WeaponData>
            {
                new WeaponData { id = 1, prefabName = "Sword", attack = 10 },
                new WeaponData { id = 2, prefabName = "Bow", attack = 8 },
            };
            ConfigTestHelper.SetField(config, "weapons", weapons);
            config.OnMappingValue();

            Assert.IsTrue(config.TryGetWeaponData(1, out var w1));
            Assert.AreEqual("Sword", w1.prefabName);
            Assert.IsTrue(config.TryGetWeaponData(2, out var w2));
            Assert.AreEqual("Bow", w2.prefabName);
        }

        [Test]
        public void OnMappingValue_BuildsUpgradeCache()
        {
            var config = new WeaponConfig();
            var upgrades = new List<WeaponUpgradeData>
            {
                new WeaponUpgradeData { id = 1, level = 1, type = UpgradeType.LevelUp, damagePercent = 10 },
                new WeaponUpgradeData { id = 1, level = 2, type = UpgradeType.LevelUp, damagePercent = 20 },
            };
            ConfigTestHelper.SetField(config, "upgrades", upgrades);
            config.OnMappingValue();

            Assert.IsTrue(config.TryGetUpgradeWeapon(1, 1, UpgradeType.LevelUp, out var list1));
            Assert.AreEqual(1, list1.Count);
            Assert.AreEqual(10f, list1[0].damagePercent);

            Assert.IsTrue(config.TryGetUpgradeWeapon(1, 2, UpgradeType.LevelUp, out var list2));
            Assert.AreEqual(20f, list2[0].damagePercent);
        }

        [Test]
        public void TryGetUpgradeWeapon_MissingKey_ReturnsFalse()
        {
            var config = new WeaponConfig();
            config.OnMappingValue();
            Assert.IsFalse(config.TryGetUpgradeWeapon(99, 1, UpgradeType.LevelUp, out _));
        }

        [Test]
        public void TryGetWeaponData_MissingId_ReturnsFalse()
        {
            var config = new WeaponConfig();
            config.OnMappingValue();
            Assert.IsFalse(config.TryGetWeaponData(99, out _));
        }
        
        [Test]
        public void OnPostImported_MergesScaleAndBase()
        {
            var config = new MonsterConfig();
            var baseList = new List<BaseMonsterData>
            {
                new BaseMonsterData
                {
                    id = "goblin_base", prefabName = "Goblin", speed = 2f, radius = 0.5f,
                    attack = 10, health = 50, exp = 20, deathAudioClip = "goblin_death", deathVolume = 0.8f
                }
            };
            var scaleList = new List<MonsterScaleData>
            {
                new MonsterScaleData
                {
                    id = 1, baseId = "goblin_base", scaleSpeed = 1.2f, scaleRadius = 1.5f,
                    healthScale = 2f, attackScale = 1.5f, expScale = 1.0f,
                    stopDistance = 0.3f
                }
            };
            ConfigTestHelper.SetField(config, "monster_base", baseList);
            ConfigTestHelper.SetField(config, "monster_scale", scaleList);

            config.OnMappingValue();
            config.OnPostImported();
            config.OnMappingValue();

            Assert.IsTrue(config.TryGetMonsterData(1, out var result));
            Assert.AreEqual("Goblin", result.prefabName);
            Assert.AreEqual(2.4f, result.speed);      // 2 * 1.2
            Assert.AreEqual(0.75f, result.radius);    // 0.5 * 1.5
            Assert.AreEqual(1.5f, result.scale);      // scaleRadius
            Assert.AreEqual(100f, result.health);     // 50 * 2
            Assert.AreEqual(15f, result.attack);      // 10 * 1.5
            Assert.AreEqual(20f, result.exp);         // 20 * 1.0
            Assert.AreEqual(0.3f, result.stopDistance);
            Assert.AreEqual("goblin_death", result.deathAudioClip);
            Assert.AreEqual(0.8f, result.deathVolume);
        }

        [Test]
        public void TryGetMonsterData_MissingId_ReturnsFalse()
        {
            var config = new MonsterConfig();
            var monsters = new List<MonsterData> { new MonsterData { id = 10 } };
            ConfigTestHelper.SetField(config, "monsters", monsters);
            config.OnMappingValue();

            Assert.IsFalse(config.TryGetMonsterData(99, out _));
        }
        
        [Test]
        public void TryGetExp_MissingLevel_ReturnsFalse()
        {
            var config = new PlayerConfig();
            var data = new List<PlayerExpData> { new PlayerExpData { level = 5, exp = 500 } };
            ConfigTestHelper.SetField(config, "exp", data);
            config.OnMappingValue();

            Assert.IsFalse(config.TryGetExp(1, out _));
            Assert.IsFalse(config.TryGetExp(99, out _));
        }
        
        [Test]
        public void OnMappingValue_EmptyList_CacheEmpty()
        {
            var config = new PlayerConfig();
            config.OnMappingValue();
            bool found = config.TryGetExp(1, out _);
            Assert.IsFalse(found);
        }
        
        [Test]
        public void OnMappingValue_WithData_TryGetExpReturnsCorrect()
        {
            var config = new PlayerConfig();
            var data = new List<PlayerExpData>
            {
                new PlayerExpData { level = 1, exp = 100 },
                new PlayerExpData { level = 2, exp = 250 },
            };
            ConfigTestHelper.SetField(config, "exp", data);
            config.OnMappingValue();

            Assert.IsTrue(config.TryGetExp(1, out var result));
            Assert.AreEqual(100, result.exp);
            Assert.IsTrue(config.TryGetExp(2, out result));
            Assert.AreEqual(250, result.exp);
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

        [Test]
        public void LinearFormula_Test()
        {
            LinearFormula formula;

            formula = new LinearFormula(0, 1, 2);
            Assert.AreEqual(formula.Evaluate(2), 4);
            Assert.AreEqual(formula.Evaluate(9), 11);

            formula = new LinearFormula(0, 3, 4);
            Assert.AreEqual(formula.Evaluate(3), 13);
            Assert.AreEqual(formula.Evaluate(9), 31);
        }

        [Test]
        public void PowerFormula_Test()
        {
            PowerFormula formula;
            
            formula = new PowerFormula(0, 1, 2);
            Assert.AreEqual(formula.Evaluate(2), 4);
            Assert.AreEqual(formula.Evaluate(9), 81);
            
            formula = new PowerFormula(0, 3, 4);
            Assert.AreEqual(formula.Evaluate(3), 243);
            Assert.AreEqual(formula.Evaluate(9), 19683);
        }
    }
}
#endif