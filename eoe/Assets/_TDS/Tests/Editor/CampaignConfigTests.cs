using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using _TDS.GameConfig;
using _TDS.Gameplay;

namespace _TDS.Tests.Editor
{
    public sealed class CampaignConfigTests
    {
        [Test]
        public void LevelThreeIntroducesTheUpgradeRequiredDifficultySpike()
        {
            string path = Path.Combine(Application.dataPath, "_TDSAssets/Config/SpawnConfig.json");
            SpawnFile file = JsonUtility.FromJson<SpawnFile>(File.ReadAllText(path));
            SpawnRow boss = null;
            float levelTwoHealth = 0f;
            foreach (SpawnRow row in file.spawns)
            {
                if (row.definition.level == 2) levelTwoHealth = Mathf.Max(levelTwoHealth, row.scale.healthMultiplier);
                if (row.definition.level == 3 && row.definition.wave == 5) boss = row;
            }

            Assert.That(levelTwoHealth, Is.LessThanOrEqualTo(0.9f));
            Assert.That(boss, Is.Not.Null);
            Assert.That(boss.monsterId, Is.EqualTo(1003));
            Assert.That(boss.scale.healthMultiplier, Is.GreaterThanOrEqualTo(1.8f));
            Assert.That(boss.scale.attackMultiplier, Is.GreaterThanOrEqualTo(1.5f));
        }

        [Test]
        public void SpawnConfigDefinesTwentyFiveWaveCampaign()
        {
            string path = Path.Combine(Application.dataPath, "_TDSAssets/Config/SpawnConfig.json");
            SpawnFile file = JsonUtility.FromJson<SpawnFile>(File.ReadAllText(path));
            Assert.That(file, Is.Not.Null);
            Assert.That(file.spawns, Is.Not.Null);

            HashSet<int> monsterIds = new HashSet<int> { 1001, 1002, 1003 };
            for (int level = RunSelection.DefaultLevel; level <= RunSelection.MaxCampaignLevel; level++)
            {
                HashSet<int> waves = new HashSet<int>();
                foreach (SpawnRow row in file.spawns)
                {
                    if (row.definition.level != level) continue;
                    waves.Add(row.definition.wave);
                    Assert.That(row.total, Is.GreaterThan(0));
                    Assert.That(monsterIds.Contains(row.monsterId), Is.True);
                    Assert.That(row.portals, Is.Not.Null.And.Not.Empty);
                    Assert.That(row.spawnsTime, Has.Length.EqualTo(2));
                }

                Assert.That(waves, Is.EquivalentTo(new[] { 1, 2, 3, 4, 5 }), $"Level {level} waves");
            }
        }

        [System.Serializable]
        private sealed class SpawnFile
        {
            public List<SpawnRow> spawns;
        }

        [System.Serializable]
        private sealed class SpawnRow
        {
            public SpawnDefinition definition;
            public int monsterId;
            public int total;
            public SpawnScaleDefinition scale;
            public float[] spawnsTime;
            public int[] portals;
        }
    }
}
