using System.IO;
using _TDS.Battle;
using _TDS.GameConfig;
using Newtonsoft.Json;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace _TDS.Tests.Editor
{
    public sealed class CombatCatalogCoverageTests
    {
        [Test]
        public void EveryConfiguredHeroHasRuntimePrefabAndSkill()
        {
            HeroConfigFile config = Load<HeroConfigFile>("Assets/_TDSAssets/Config/HeroConfig.json");
            SkillConfigFile skills = Load<SkillConfigFile>("Assets/_TDSAssets/Config/SkillConfig.json");
            Assert.That(config?.heros, Is.Not.Null.And.Not.Empty);
            Assert.That(skills?.skills, Is.Not.Null.And.Not.Empty);

            foreach (HeroConfigData hero in config.heros)
            {
                Assert.That(hero.prefabName, Is.Not.Null.And.Not.Empty, $"Hero {hero.id} has no prefab");
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                    $"Assets/_TDSAssets/Battles/Heros/{hero.prefabName}.prefab");
                Assert.That(prefab, Is.Not.Null, $"Missing hero prefab {hero.prefabName}");
                Assert.That(prefab.GetComponent<Hero>(), Is.Not.Null, $"Hero prefab {hero.prefabName} needs Hero");
                Assert.That(ArrayContainsSkill(skills.skills, hero.attackId), Is.True,
                    $"Hero {hero.id} references missing skill {hero.attackId}");
            }
        }

        [Test]
        public void EveryConfiguredMonsterHasRuntimePrefab()
        {
            MonsterConfigFile config = Load<MonsterConfigFile>("Assets/_TDSAssets/Config/MonsterConfig.json");
            Assert.That(config?.monsters, Is.Not.Null.And.Not.Empty);

            foreach (MonsterConfigData monster in config.monsters)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                    $"Assets/_TDSAssets/Battles/Monsters/{monster.prefabName}.prefab");
                Assert.That(prefab, Is.Not.Null, $"Missing monster prefab {monster.id}");
                Assert.That(prefab.GetComponent<Monster>(), Is.Not.Null,
                    $"Monster prefab {monster.prefabName} needs Monster");
            }
        }

        private static T Load<T>(string path)
        {
            return JsonConvert.DeserializeObject<T>(File.ReadAllText(path));
        }

        private static bool ArrayContainsSkill(SkillConfigData[] skills, int skillId)
        {
            for (int i = 0; i < skills.Length; i++)
            {
                if (skills[i].skillId == skillId) return true;
            }

            return false;
        }

        private sealed class HeroConfigFile
        {
            public HeroConfigData[] heros;
        }

        private sealed class SkillConfigFile
        {
            public SkillConfigData[] skills;
        }

        private sealed class MonsterConfigFile
        {
            public MonsterConfigData[] monsters;
        }
    }
}
