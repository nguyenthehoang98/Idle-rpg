#if UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using _GameToolkit.SkillSystem.Core;
using NUnit.Framework;

namespace _GameToolkit.SkillSystem.Tests
{
    public class SpuTests
    {
        private sealed class TestAction : BaseAction
        {
            public int UpdateCount { get; private set; }

            public TestAction(float lifeTime) : base(lifeTime) { }

            protected override void OnUpdate(float deltaTime) => UpdateCount++;
        }

        private sealed class LifecycleProbeAction : BaseAction
        {
            private readonly System.Action onStop;

            public LifecycleProbeAction(float lifeTime, System.Action onStop) : base(lifeTime)
            {
                this.onStop = onStop;
            }

            protected override void OnStop() => onStop();
        }

        [Test]
        public void RequestAddAction_AfterTick_HasAction()
        {
            var spu = new Spu();
            int id = -1;

            spu.RequestAddAction(1, new TestAction(10f), added => id = added);
            spu.Tick(0f);

            Assert.That(id, Is.GreaterThanOrEqualTo(1));
            Assert.IsTrue(spu.HasAction(id));
        }

        [Test]
        public void Tick_LifeCycleFinishedAction_Removed()
        {
            var spu = new Spu();
            int id = -1;

            spu.RequestAddAction(1, new TestAction(0.5f), added => id = added);
            spu.Tick(1f);

            Assert.IsFalse(spu.HasAction(id));
        }

        [Test]
        public void RequestRemoveAction_RemovesAction()
        {
            var spu = new Spu();
            int id = -1;
            bool removed = false;

            spu.RequestAddAction(1, new TestAction(10f), added => id = added);
            spu.Tick(0f);

            spu.RequestRemoveAction(id, r => removed = r);
            spu.Tick(0f);

            Assert.IsTrue(removed);
            Assert.IsFalse(spu.HasAction(id));
        }

        [Test]
        public void RequestRemoveAction_Twice_SecondReturnsFalse()
        {
            var spu = new Spu();
            int id = -1;
            bool first = false;
            bool second = false;

            spu.RequestAddAction(1, new TestAction(10f), added => id = added);
            spu.Tick(0f);

            spu.RequestRemoveAction(id, r => first = r);
            spu.Tick(0f);

            spu.RequestRemoveAction(id, r => second = r);
            spu.Tick(0f);

            Assert.IsTrue(first);
            Assert.IsFalse(second);
        }

        [Test]
        public void HasSkill_GroupsActionsBySkillId()
        {
            var spu = new Spu();

            spu.RequestAddAction(7, new TestAction(10f));
            spu.RequestAddAction(7, new TestAction(10f));
            spu.Tick(0f);

            Assert.IsTrue(spu.HasSkill(7, out List<int> list));
            Assert.AreEqual(2, list.Count);
            Assert.IsFalse(spu.HasSkill(8, out _));
        }

        [Test]
        public void SkillId_ReusedAfterAllActionsRemoved()
        {
            var spu = new Spu();

            spu.RequestAddAction(5, new TestAction(0.1f));
            spu.Tick(1f); // action finish -> remove -> freeIds chứa 5

            Assert.AreEqual(5, spu.GenerateSkillInstanceId());
        }

        [Test]
        public void TryGetAction_ReturnsLiveAction()
        {
            var spu = new Spu();
            int id = -1;
            var action = new TestAction(10f);

            spu.RequestAddAction(1, action, added => id = added);
            spu.Tick(0f);

            Assert.IsTrue(spu.TryGetAction(id, out IAction found));
            Assert.AreSame(action, found);
        }

        [Test]
        public void RequestRemoveAction_InterruptsThenStops()
        {
            var spu = new Spu();
            int id = -1;
            bool stopCalled = false;
            var action = new LifecycleProbeAction(10f, () => stopCalled = true);

            spu.RequestAddAction(1, action, added => id = added);
            spu.Tick(0f);

            spu.RequestRemoveAction(id);
            spu.Tick(0f);

            Assert.AreEqual(ActionCompleteReason.Interrupt, action.Reason);
            Assert.IsTrue(action.IsFinished);
            Assert.IsTrue(stopCalled);
        }
    }
}
#endif
