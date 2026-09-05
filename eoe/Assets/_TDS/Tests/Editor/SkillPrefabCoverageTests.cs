using System.IO;
using _GameToolkit.Colliders;
using _TDS.Battle;
using _TDS.GameConfig;
using Newtonsoft.Json;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace _TDS.Tests.Editor
{
    public sealed class SkillPrefabCoverageTests
    {
        private const string ConfigPath = "Assets/_TDSAssets/Config/SkillConfig.json";
        private const string PrefabFolder = "Assets/_TDSAssets/Battles/Projectiles";

        [Test]
        public void EveryConfiguredSkillHasRequiredProjectileComponents()
        {
            string json = File.ReadAllText(ConfigPath);
            SkillConfigFile config = JsonConvert.DeserializeObject<SkillConfigFile>(json);

            Assert.That(config?.skills, Is.Not.Null.And.Not.Empty);

            foreach (SkillConfigData skill in config.skills)
            {
                Assert.That(skill.prefabName, Is.Not.Null.And.Not.Empty, $"Skill {skill.skillId} has no prefab");

                string path = $"{PrefabFolder}/{skill.prefabName}.prefab";
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                Assert.That(prefab, Is.Not.Null, $"Missing prefab for skill {skill.skillId}: {path}");
                Assert.That(prefab.GetComponent<Projectile>(), Is.Not.Null,
                    $"Prefab {skill.prefabName} needs a Projectile component");
                Assert.That(prefab.GetComponentsInChildren<CollisionDetector>(true), Is.Not.Empty,
                    $"Prefab {skill.prefabName} needs a CollisionDetector");
            }
        }

        private sealed class SkillConfigFile
        {
            public SkillConfigData[] skills;
        }
    }
}
