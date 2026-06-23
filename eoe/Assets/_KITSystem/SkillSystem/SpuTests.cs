#if UNITY_INCLUDE_TESTS
using _KITSystem.SkillSystem.Core;
using NUnit.Framework;

namespace _KITSystem.SkillSystem
{
    public class SpuTests
    {
        private class MockAction : IAction
        {
            public int StartCallCount { get; private set; }
            public int TickCallCount { get; private set; }
            public int StopCallCount { get; private set; }
            public int InterruptCallCount { get; private set; }

            public bool IsFinished { get; set; }
            public ActionCompleteReason Reason { get; private set; }

            public void Start() { StartCallCount++; }
            public void Tick(float deltaTime) { TickCallCount++; }
            public void Interrupt() { InterruptCallCount++; Reason = ActionCompleteReason.Interrupt; IsFinished = true; }
            public void Stop() { StopCallCount++; }

            public void MarkFinished()
            {
                IsFinished = true;
                Reason = ActionCompleteReason.EndLifeCycle;
            }
        }

        [Test]
        public void RequestAddAction_AddsAndTicksAction()
        {
            var spu = new Spu();
            var action = new MockAction();

            spu.RequestAddAction(1, action);
            spu.Tick(0.016f);

            Assert.AreEqual(1, action.StartCallCount);
            Assert.AreEqual(1, action.TickCallCount);
        }

        [Test]
        public void RequestRemoveAction_InterruptsAndStopsAction()
        {
            var spu = new Spu();
            var action = new MockAction();

            spu.RequestAddAction(1, action, id => spu.RequestRemoveAction(id));
            spu.Tick(0.016f);

            Assert.AreEqual(1, action.InterruptCallCount);
            Assert.AreEqual(1, action.StopCallCount);
        }

        [Test]
        public void FinishedAction_RemovedAfterTick()
        {
            var spu = new Spu();
            var action = new MockAction();
            bool removed = false;

            spu.RequestAddAction(1, action, id =>
            {
                action.MarkFinished();
                spu.Tick(0.016f);
                removed = !spu.HasAction(id);
            });
            spu.Tick(0.016f);

            Assert.IsTrue(removed);
        }

        [Test]
        public void MultipleSkills_DoNotInterfere()
        {
            var spu = new Spu();
            var a1 = new MockAction();
            var a2 = new MockAction();

            spu.RequestAddAction(1, a1);
            spu.RequestAddAction(2, a2);
            spu.Tick(0.016f);

            Assert.AreEqual(1, a1.StartCallCount);
            Assert.AreEqual(1, a1.TickCallCount);
            Assert.AreEqual(1, a2.StartCallCount);
            Assert.AreEqual(1, a2.TickCallCount);
        }

        [Test]
        public void HasSkill_ReturnsFalseForUnknownSkill()
        {
            var spu = new Spu();
            Assert.IsFalse(spu.HasSkill(99, out _));
        }

        [Test]
        public void TryGetAction_ReturnsActionById()
        {
            var spu = new Spu();
            var action = new MockAction();
            int addedId = 0;

            spu.RequestAddAction(1, action, id => addedId = id);
            spu.Tick(0.016f);

            bool found = spu.TryGetAction(addedId, out IAction retrieved);
            Assert.IsTrue(found);
            Assert.AreSame(action, retrieved);
        }

        [Test]
        public void TryGetAction_UnknownId_ReturnsFalse()
        {
            var spu = new Spu();
            bool found = spu.TryGetAction(999, out _);
            Assert.IsFalse(found);
        }

        [Test]
        public void GenerateSkillInstanceId_ReturnsUniqueIds()
        {
            var spu = new Spu();
            int id1 = spu.GenerateSkillInstanceId();
            int id2 = spu.GenerateSkillInstanceId();
            Assert.AreNotEqual(id1, id2);
        }
    }
}
#endif
