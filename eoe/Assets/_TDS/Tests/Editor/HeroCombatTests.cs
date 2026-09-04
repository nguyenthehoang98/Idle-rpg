using NUnit.Framework;
using _TDS.Battle;

namespace _TDS.Tests.Editor
{
    public class HeroCombatTests
    {
        [Test]
        public void MonsterStopsInsideHeroAttackRange()
        {
            float stopDistance = AgentMovementRunner.CalculateStopDistance(
                monsterAttackRange: 2f,
                monsterRadius: 0.25f,
                heroAttackRange: 1f);

            Assert.That(stopDistance, Is.EqualTo(1f));
        }

        [Test]
        public void MonsterRadiusStillPreventsInvalidOverlap()
        {
            float stopDistance = AgentMovementRunner.CalculateStopDistance(
                monsterAttackRange: 2f,
                monsterRadius: 1.5f,
                heroAttackRange: 1f);

            Assert.That(stopDistance, Is.EqualTo(1.5f));
        }
    }
}
