#if UNITY_INCLUDE_TESTS
using _TDS.Gameplay;
using NUnit.Framework;

namespace _TDS.Tests
{
    public class SpawnTimerTests
    {
        [Test]
        public void Spawn_BeforeStartTime_ReturnsZero()
        {
            var timer = new SpawnTimer(2f, 10f, 100);

            Assert.AreEqual(0, timer.Spawn(1f));
            Assert.AreEqual(0, timer.Spawn(0.5f));
        }

        [Test]
        public void Spawn_AfterEndTime_ReturnsAllRemaining()
        {
            var timer = new SpawnTimer(0f, 5f, 10);

            // tick lớn vượt hẳn window → trả toàn bộ 10
            int count = timer.Spawn(10f);

            Assert.AreEqual(10, count);
            Assert.IsTrue(timer.IsFinished);
        }

        [Test]
        public void Spawn_LinearProgress_TotalIsExact()
        {
            var timer = new SpawnTimer(0f, 10f, 100);

            int totalSpawned = 0;

            // 10 ticks, mỗi tick 1s → tổng = 100
            for (int i = 0; i < 10; i++)
            {
                totalSpawned += timer.Spawn(1f);
            }

            Assert.AreEqual(100, totalSpawned);
            Assert.IsTrue(timer.IsFinished);
        }

        [Test]
        public void Spawn_NeverReturnsNegative()
        {
            var timer = new SpawnTimer(0f, 5f, 10);

            int totalSpawned = 0;

            for (int i = 0; i < 20; i++)
            {
                totalSpawned += timer.Spawn(0.1f);

                Assert.That(totalSpawned, Is.GreaterThanOrEqualTo(0));
                Assert.That(totalSpawned, Is.LessThanOrEqualTo(10));
            }

            Assert.AreEqual(10, totalSpawned);
        }

        [Test]
        public void Spawn_ZeroDuration_ReturnsZero()
        {
            var timer = new SpawnTimer(0f, 0f, 10);

            Assert.AreEqual(0, timer.Spawn(5f));
        }

        [Test]
        public void Spawn_ZeroTotal_ReturnsZero()
        {
            var timer = new SpawnTimer(0f, 5f, 0);

            Assert.AreEqual(0, timer.Spawn(5f));
        }

        [Test]
        public void IsFinished_InitiallyFalse()
        {
            var timer = new SpawnTimer(0f, 5f, 10);

            Assert.IsFalse(timer.IsFinished);
        }

        [Test]
        public void IsFinished_OnlyWhenAllSpawned()
        {
            var timer = new SpawnTimer(0f, 1f, 3);

            timer.Spawn(0.4f);  // ~1 con
            Assert.IsFalse(timer.IsFinished);

            timer.Spawn(0.4f);  // ~2 con
            Assert.IsFalse(timer.IsFinished);

            timer.Spawn(0.4f);  // 3 con
            Assert.IsTrue(timer.IsFinished);
        }
    }
}
#endif
