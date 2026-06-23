#if UNITY_INCLUDE_TESTS
using NUnit.Framework;

namespace _Game.GamePlay.SpawnerSystem
{
    public class SpawnTimerTests
    {
        [Test]
        public void Spawn_NegativeElapsedTime_ReturnsZero()
        {
            var timer = new SpawnTimer(startTime: 5f, endTime: 10f, total: 10);
            int result = timer.Spawn(1f);
            Assert.AreEqual(0, result);
        }

        [Test]
        public void Spawn_ElapsedBelowStartTime_ReturnsZero()
        {
            var timer = new SpawnTimer(startTime: 5f, endTime: 10f, total: 10);
            int result = timer.Spawn(3f);
            Assert.AreEqual(0, result);
        }

        [Test]
        public void Spawn_AfterStartTime_ReturnsNonZero()
        {
            var timer = new SpawnTimer(startTime: 0f, endTime: 10f, total: 10);
            int result = timer.Spawn(5f);
            Assert.Greater(result, 0);
        }

        [Test]
        public void Spawn_ZeroDuration_ReturnsZero()
        {
            var timer = new SpawnTimer(startTime: 0f, endTime: 0f, total: 10);
            int result = timer.Spawn(5f);
            Assert.AreEqual(0, result);
        }

        [Test]
        public void Spawn_ZeroTotal_ReturnsZero()
        {
            var timer = new SpawnTimer(startTime: 0f, endTime: 10f, total: 0);
            int result = timer.Spawn(5f);
            Assert.AreEqual(0, result);
        }

        [Test]
        public void Spawn_ProgressMidDuration_ReturnsExpectedCount()
        {
            var timer = new SpawnTimer(startTime: 0f, endTime: 10f, total: 100);
            int result = timer.Spawn(5f);
            Assert.AreEqual(50, result);
        }

        [Test]
        public void Spawn_CumulativeCall_AccumulatesCorrectly()
        {
            var timer = new SpawnTimer(startTime: 0f, endTime: 10f, total: 100);
            timer.Spawn(2f);
            int r2 = timer.Spawn(3f);
            Assert.AreEqual(30, r2);
        }

        [Test]
        public void Spawn_AfterCompletion_ReturnsZero()
        {
            var timer = new SpawnTimer(startTime: 0f, endTime: 10f, total: 10);
            timer.Spawn(10f);
            int result = timer.Spawn(1f);
            Assert.AreEqual(0, result);
        }

        [Test]
        public void IsFinished_WhenSpawnedAllTotal_ReturnsTrue()
        {
            var timer = new SpawnTimer(startTime: 0f, endTime: 10f, total: 10);
            timer.Spawn(10f);
            Assert.IsTrue(timer.IsFinished);
        }

        [Test]
        public void IsFinished_BeforeCompletion_ReturnsFalse()
        {
            var timer = new SpawnTimer(startTime: 0f, endTime: 10f, total: 10);
            timer.Spawn(5f);
            Assert.IsFalse(timer.IsFinished);
        }

        [Test]
        public void Spawn_PositiveTimeElapsedAfterManyCalls_MaintainsCorrectness()
        {
            var timer = new SpawnTimer(startTime: 2f, endTime: 7f, total: 50);
            int t1 = timer.Spawn(1f);
            Assert.AreEqual(0, t1);
            int t2 = timer.Spawn(1.5f);
            Assert.Greater(t2, 0);
            int t3 = timer.Spawn(2f);
            Assert.IsFalse(timer.IsFinished);
        }

        [Test]
        public void Spawn_StartTimeEqualsElapsed_StartsSpawning()
        {
            var timer = new SpawnTimer(startTime: 5f, endTime: 10f, total: 20);
            int result = timer.Spawn(5f);
            Assert.AreEqual(result, 0);
        }

        [Test]
        public void Spawn_EndTimeEqualsStartTime_ReturnsZero()
        {
            var timer = new SpawnTimer(startTime: 5f, endTime: 5f, total: 10);
            timer.Spawn(3f);
            int result = timer.Spawn(5f);
            Assert.AreEqual(0, result);
        }

        [Test]
        public void Spawn_SingleMonsterTotal_CompletesInOneSpawn()
        {
            var timer = new SpawnTimer(startTime: 0f, endTime: 1f, total: 1);
            int result = timer.Spawn(1f);
            Assert.AreEqual(1, result);
            Assert.IsTrue(timer.IsFinished);
        }

        [Test]
        public void Spawn_ProgressPastDuration_CompletesTotal()
        {
            var timer = new SpawnTimer(startTime: 0f, endTime: 5f, total: 25);
            timer.Spawn(10f);
            Assert.IsTrue(timer.IsFinished);
        }
    }
}
#endif
