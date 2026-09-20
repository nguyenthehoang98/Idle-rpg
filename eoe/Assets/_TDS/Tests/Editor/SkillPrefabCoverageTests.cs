using System.IO;
using _GameToolkit.Colliders;
using _TDS.Battle;
using _TDS.GameConfig;
using Newtonsoft.Json;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.AddressableAssets;
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
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            Assert.That(settings, Is.Not.Null, "Addressable settings missing");
            string match = null;
            int count = 0;
            foreach (var group in settings.groups)
            {
                if (group == null) continue;
                foreach (var entry in group.entries)
                {
                    if (!entry.address.Equals(prefabName)) continue;
                    match = entry.AssetPath;
                    count++;
                }
            }
            Assert.That(count, Is.EqualTo(1), $"Expected exactly one addressable named {prefabName}");
            return AssetDatabase.LoadAssetAtPath<GameObject>(match);
        }

        private sealed class SkillConfigFile
        {
            public SkillConfigData[] skills;
        }
    }
}
