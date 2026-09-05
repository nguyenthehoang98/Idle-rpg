using System;
using NUnit.Framework;
using _TDS.Gameplay;

namespace _TDS.Tests.Editor
{
    public sealed class GameProgressTests
    {
        [Test]
        public void OfflineTimeIsCappedAndDoesNotRunBackwards()
        {
            long now = DateTime.UtcNow.Ticks;

            Assert.That(
                GameProgress.CalculateOfflineMinutes(now - TimeSpan.FromHours(24).Ticks, now),
                Is.EqualTo(480));
            Assert.That(GameProgress.CalculateOfflineMinutes(now, now - 1), Is.EqualTo(0));
        }

        [Test]
        public void VictoryRunPersistsProgressAndUnlocksNextLevel()
        {
            GameProgress.Reset();
            BattleRunRewards rewards = new BattleRunRewards();
            rewards.Add(10, 20);
            rewards.RecordUpgrade(30001);

            GameProgress.SaveRun(1, new[] { 101, 102 }, rewards, victory: true);

            Assert.That(GameProgress.State.campaignLevel, Is.EqualTo(2));
            Assert.That(GameProgress.State.totalExperience, Is.EqualTo(10));
            Assert.That(GameProgress.State.totalGold, Is.EqualTo(20));
            Assert.That(GameProgress.State.selectedHeroIds, Is.EqualTo(new[] { 101, 102 }));
            Assert.That(GameProgress.State.selectedUpgradeIds, Is.EqualTo(new[] { 30001 }));
            GameProgress.Reset();
        }
    }
}
