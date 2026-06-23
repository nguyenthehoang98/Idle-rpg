#if UNITY_INCLUDE_TESTS
using _KITSystem.SkillSystem.Core;
using NUnit.Framework;

namespace _KITSystem.SkillSystem
{
    public class BaseActionTests
    {
        private class TestAction : BaseAction
        {
            public int StartCallCount { get; private set; }
            public int UpdateCallCount { get; private set; }
            public int StopCallCount { get; private set; }

            public TestAction(Spu spu, float lifeTime) : base(spu, lifeTime) { }

            protected override void OnStart() { StartCallCount++; }
            protected override void OnUpdate(float deltaTime) { UpdateCallCount++; }
            protected override void OnStop() { StopCallCount++; }
        }

        [Test]
        public void Start_CallsOnStart()
        {
            var action = new TestAction(new Spu(), 10f);
            action.Start();
            Assert.AreEqual(1, action.StartCallCount);
        }

        [Test]
        public void Tick_IncrementsUpdate()
        {
            TestAction action = new TestAction(new Spu(), 10f);
            action.Start();
            action.Tick(1f);
            Assert.AreEqual(1, action.UpdateCallCount);
        }

        [Test]
        public void Tick_AfterLifeTime_EndsLifeCycle()
        {
            var action = new TestAction(new Spu(), 1f);
            action.Start();
            action.Tick(1.5f);
            Assert.IsTrue(action.IsFinished);
            Assert.AreEqual(ActionCompleteReason.EndLifeCycle, action.Reason);
        }

        [Test]
        public void Interrupt_SetsReasonAndFinishes()
        {
            var action = new TestAction(new Spu(), 10f);
            action.Start();
            action.Interrupt();
            Assert.IsTrue(action.IsFinished);
            Assert.AreEqual(ActionCompleteReason.Interrupt, action.Reason);
        }

        [Test]
        public void Stop_CallsOnStop()
        {
            var action = new TestAction(new Spu(), 10f);
            action.Start();
            action.Stop();
            Assert.AreEqual(1, action.StopCallCount);
        }

        [Test]
        public void Tick_WhenFinished_DoesNotUpdate()
        {
            var action = new TestAction(new Spu(), 1f);
            action.Start();
            action.Tick(2f);
            int before = action.UpdateCallCount;
            action.Tick(1f);
            Assert.AreEqual(before, action.UpdateCallCount);
        }

        [Test]
        public void EndLifeCycle_SetsReason()
        {
            var action = new TestAction(new Spu(), 10f);
            action.Start();
            action.EndLifeCycle();
            Assert.AreEqual(ActionCompleteReason.EndLifeCycle, action.Reason);
            Assert.IsTrue(action.IsFinished);
        }

        [Test]
        public void EndLifeCycle_OnceOnly()
        {
            var action = new TestAction(new Spu(), 10f);
            action.Start();
            action.EndLifeCycle();
            action.EndLifeCycle();
            Assert.AreEqual(ActionCompleteReason.EndLifeCycle, action.Reason);
        }
    }
}
#endif
