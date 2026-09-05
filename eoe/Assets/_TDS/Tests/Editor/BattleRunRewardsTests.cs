using NUnit.Framework;
using _TDS.Gameplay;

namespace _TDS.Tests.Editor
{
    public class BattleRunRewardsTests
    {
        [Test]
        public void AddAccumulatesExperienceGoldAndDefeatedMonsters()
        {
            BattleRunRewards rewards = new BattleRunRewards();

            rewards.Add(25, 7);
            rewards.Add(10, 3);

            Assert.That(rewards.Experience, Is.EqualTo(35));
            Assert.That(rewards.Gold, Is.EqualTo(10));
            Assert.That(rewards.DefeatedMonsters, Is.EqualTo(2));
        }

        [Test]
        public void AddIgnoresNegativeRewardValues()
        {
            BattleRunRewards rewards = new BattleRunRewards();

            rewards.Add(-5, -2);

            Assert.That(rewards.Experience, Is.EqualTo(0));
            Assert.That(rewards.Gold, Is.EqualTo(0));
            Assert.That(rewards.DefeatedMonsters, Is.EqualTo(1));
        }
    }
}
