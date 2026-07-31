#if UNITY_INCLUDE_TESTS
using _GameToolkit.SkillSystem.Core;
using NUnit.Framework;

namespace _GameToolkit.SkillSystem
{
    public class BaseActionTests
    {
        private class TestAction : BaseAction
        {
            public int StartCallCount { get; private set; }
            public int UpdateCallCount { get; private set; }
            public int StopCallCount { get; private set; }

            public TestAction(float lifeTime) : base(lifeTime) { }

            protected override void OnStart() { StartCallCount++; }
            protected override void OnUpdate(float deltaTime) { UpdateCallCount++; }
            protected override void OnStop() { StopCallCount++; }
        }

        [Test]
        public void Start_CallsOnStart()
        {
            var action = new TestAction(10f);
            action.Start();
            Assert.AreEqual(1, action.StartCallCount);
        }

        [Test]
        public void Tick_IncrementsUpdate()
        {
            TestAction action = new TestAction(10f);
            action.Start();
            action.Tick(1f);
            Assert.AreEqual(1, action.UpdateCallCount);
        }

        [Test]
        public void Tick_AfterLifeTime_EndsLifeCycle()
        {
            var action = new TestAction(1f);
            action.Start();
            action.Tick(1.5f);
            Assert.IsTrue(action.IsFinished);
            Assert.AreEqual(ActionCompleteReason.EndLifeCycle, action.Reason);
        }

        [Test]
        public void Interrupt_SetsReasonAndFinishes()
        {
            var action = new TestAction(10f);
            action.Start();
            action.Interrupt();
            Assert.IsTrue(action.IsFinished);
            Assert.AreEqual(ActionCompleteReason.Interrupt, action.Reason);
        }

        [Test]
        public void Stop_CallsOnStop()
        {
            var action = new TestAction(10f);
            action.Start();
            action.Stop();
            Assert.AreEqual(1, action.StopCallCount);
        }

        [Test]
        public void Tick_WhenFinished_DoesNotUpdate()
        {
            var action = new TestAction(1f);
            action.Start();
            action.Tick(2f);
            int before = action.UpdateCallCount;
            action.Tick(1f);
            Assert.AreEqual(before, action.UpdateCallCount);
        }

        [Test]
        public void EndLifeCycle_SetsReason()
        {
            var action = new TestAction(10f);
            action.Start();
            action.EndLifeCycle();
            Assert.AreEqual(ActionCompleteReason.EndLifeCycle, action.Reason);
            Assert.IsTrue(action.IsFinished);
        }

        [Test]
        public void EndLifeCycle_OnceOnly()
        {
            var action = new TestAction(10f);
            action.Start();
            action.EndLifeCycle();
            action.EndLifeCycle();
            Assert.AreEqual(ActionCompleteReason.EndLifeCycle, action.Reason);
        }

        [Test]
        public void Interrupt_AlreadyFinished_DoesNotChangeReason()
        {
            var action = new TestAction(10f);
            action.Start();
            action.EndLifeCycle();
            action.Interrupt();
            Assert.AreEqual(ActionCompleteReason.EndLifeCycle, action.Reason);
        }

        [Test]
        public void Interrupt_Idempotent()
        {
            var action = new TestAction(10f);
            action.Start();
            action.Interrupt();
            action.Interrupt();
            Assert.AreEqual(ActionCompleteReason.Interrupt, action.Reason);
        }

        [Test]
        public void Start_DoesNotFinish()
        {
            var action = new TestAction(10f);
            action.Start();
            Assert.IsFalse(action.IsFinished);
            Assert.AreEqual(ActionCompleteReason.Undefined, action.Reason);
        }

        [Test]
        public void Tick_LifeTimeZero_ImmediateFinish()
        {
            var action = new TestAction(0f);
            action.Start();
            action.Tick(0f);
            Assert.IsTrue(action.IsFinished);
            Assert.AreEqual(ActionCompleteReason.EndLifeCycle, action.Reason);
        }

        [Test]
        public void Tick_ExactLifeTime_Finishes()
        {
            var action = new TestAction(5f);
            action.Start();
            action.Tick(5f);
            Assert.IsTrue(action.IsFinished);
            Assert.AreEqual(ActionCompleteReason.EndLifeCycle, action.Reason);
        }

        [Test]
        public void Tick_MultiplePartial_AccumulatesToLifeTime()
        {
            var action = new TestAction(3f);
            action.Start();

            action.Tick(1f);
            Assert.IsFalse(action.IsFinished);

            action.Tick(1f);
            Assert.IsFalse(action.IsFinished);

            action.Tick(1f);
            Assert.IsTrue(action.IsFinished);
            Assert.AreEqual(ActionCompleteReason.EndLifeCycle, action.Reason);
        }

        [Test]
        public void Tick_MultiplePartial_ExceedsLifeTime()
        {
            var action = new TestAction(3f);
            action.Start();

            action.Tick(1f);
            action.Tick(1f);
            action.Tick(1.5f);
            Assert.IsTrue(action.IsFinished);
        }

        [Test]
        public void Reason_DefaultsToUndefined()
        {
            var action = new TestAction(10f);
            Assert.AreEqual(ActionCompleteReason.Undefined, action.Reason);
        }

        [Test]
        public void Tick_WithoutStart_StillAccumulatesLifeTime()
        {
            var action = new TestAction(1f);
            action.Tick(1.5f);
            Assert.IsTrue(action.IsFinished);
        }

        [Test]
        public void Interrupt_ThenStop_StopStillCalled()
        {
            var action = new TestAction(10f);
            action.Start();
            action.Interrupt();
            action.Stop();
            Assert.AreEqual(1, action.StopCallCount);
        }

        [Test]
        public void EndLifeCycle_ThenStop_StopStillCalled()
        {
            var action = new TestAction(10f);
            action.Start();
            action.EndLifeCycle();
            action.Stop();
            Assert.AreEqual(1, action.StopCallCount);
        }

        [Test]
        public void OnUpdate_ReceivesDeltaTimeWhenNotFinished()
        {
            var action = new TestAction(5f);
            action.Start();
            action.Tick(2f);
            Assert.AreEqual(1, action.UpdateCallCount);
        }

        [Test]
        public void OnUpdate_NotCalledAfterFinished()
        {
            var action = new TestAction(1f);
            action.Start();
            action.Tick(2f);
            int before = action.UpdateCallCount;
            action.Tick(1f);
            Assert.AreEqual(before, action.UpdateCallCount);
        }

        [Test]
        public void EndLifeCycle_PreventsLifeTimeAutoFinish()
        {
            var action = new TestAction(2f);
            action.Start();
            action.EndLifeCycle();
            action.Tick(3f);
            Assert.IsTrue(action.IsFinished);
            Assert.AreEqual(ActionCompleteReason.EndLifeCycle, action.Reason);
        }

        [Test]
        public void Interrupt_PreventsLifeTimeAutoFinish()
        {
            var action = new TestAction(2f);
            action.Start();
            action.Interrupt();
            action.Tick(3f);
            Assert.IsTrue(action.IsFinished);
            Assert.AreEqual(ActionCompleteReason.Interrupt, action.Reason);
        }

        [Test]
        public void MultipleTicks_UpdateCalledEachTime()
        {
            var action = new TestAction(10f);
            action.Start();
            action.Tick(1f);
            action.Tick(1f);
            action.Tick(1f);
            Assert.AreEqual(3, action.UpdateCallCount);
        }
    }
}
#endif
