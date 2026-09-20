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

        [Test]
        public void EveryConfiguredSkillHasRequiredProjectileComponents()
        {
            string json = File.ReadAllText(ConfigPath);
            SkillConfigFile config = JsonConvert.DeserializeObject<SkillConfigFile>(json);

            Assert.That(config?.skills, Is.Not.Null.And.Not.Empty);

            foreach (SkillConfigData skill in config.skills)
            {
                Assert.That(skill.prefabName, Is.Not.Null.And.Not.Empty, $"Skill {skill.skillId} has no prefab");

                GameObject prefab = FindPrefab(skill.prefabName);
                Assert.That(prefab, Is.Not.Null, $"Missing prefab for skill {skill.skillId}: {skill.prefabName}");
                Assert.That(prefab.GetComponent<Projectile>(), Is.Not.Null,
                    $"Prefab {skill.prefabName} needs a Projectile component");
                Assert.That(prefab.GetComponentsInChildren<CollisionDetector>(true), Is.Not.Empty,
                    $"Prefab {skill.prefabName} needs a CollisionDetector");
            }
        }

        private static GameObject FindPrefab(string prefabName)
        {
            string[] guids = AssetDatabase.FindAssets($"{prefabName} t:Prefab");
            string match = null;
            int count = 0;
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!Path.GetFileNameWithoutExtension(path).Equals(prefabName)) continue;
                match = path;
                count++;
            }
            Assert.That(count, Is.EqualTo(1), $"Expected exactly one prefab named {prefabName}");
            return AssetDatabase.LoadAssetAtPath<GameObject>(match);
        }

        private sealed class SkillConfigFile
        {
            public SkillConfigData[] skills;
        }
    }
}
