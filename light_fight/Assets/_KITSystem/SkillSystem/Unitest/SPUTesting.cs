using _KITSystem.SkillSystem.Runtime;
using NUnit.Framework;

namespace _KITSystem.SkillSystem.Unitest
{
     public class SPUTesting
    {
        [Test]
        public void AddAction_ShouldExistInSystem()
        {
            var spu = CreateSPU();

            int actionId = -1;

            spu.RequestAddAction(1, new DummyAction(), id => actionId = id);
            spu.Tick(0);

            Assert.IsTrue(actionId >= 0);
        }

        [Test]
        public void RemoveAction_ShouldRemoveCorrectly()
        {
            var spu = CreateSPU();

            int actionId = -1;

            spu.RequestAddAction(1, new DummyAction(), id => actionId = id);
            spu.Tick(0);

            spu.RequestRemoveAction(actionId);
            spu.Tick(0);

            Assert.IsFalse(spu.HasAction(actionId));
        }

        [Test]
        public void RemoveAction_ShouldKeepOtherActionsValid()
        {
            var spu = CreateSPU();

            int a1 = -1, a2 = -1, a3 = -1;

            spu.RequestAddAction(1, new DummyAction(), id => a1 = id);
            spu.RequestAddAction(1, new DummyAction(), id => a2 = id);
            spu.RequestAddAction(1, new DummyAction(), id => a3 = id);
            spu.Tick(0);

            spu.RequestRemoveAction(a2);
            spu.Tick(0);

            Assert.IsTrue(spu.HasAction(a1));
            Assert.IsTrue(spu.HasAction(a3));
        }

        [Test]
        public void Request_ShouldNotApplyImmediately()
        {
            var spu = CreateSPU();

            int actionId = -1;

            spu.RequestAddAction(1, new DummyAction(), id => actionId = id);

            // chưa Tick
            Assert.AreEqual(-1, actionId);

            spu.Tick(0);

            Assert.IsTrue(actionId >= 0);
        }

        [Test]
        public void Action_ShouldAutoRemove_WhenFinished()
        {
            var spu = CreateSPU();

            int actionId = -1;

            spu.RequestAddAction(1, new FinishImmediatelyAction(), id => actionId = id);
            spu.Tick(0);

            // tick thêm để trigger remove
            spu.Tick(0);

            Assert.IsFalse(spu.HasAction(actionId));
        }

        SPU CreateSPU()
        {
            return new SPU();
        }
    }

    //========================
    // DUMMY ACTION
    //========================

    class DummyAction : IAction
    {
        public bool IsFinished => false;
        public ActionCompleteReason Reason => ActionCompleteReason.EndLifeCycle;

        public void OnStart() { }
        public void Tick(float deltaTime) { }
        public void OnEnd() { }
        public void OnInterrupt() { }
    }

    class FinishImmediatelyAction : IAction
    {
        public bool IsFinished => true;
        public ActionCompleteReason Reason => ActionCompleteReason.EndLifeCycle;

        public void OnStart() { }
        public void Tick(float deltaTime) { }
        public void OnEnd() { }
        public void OnInterrupt() { }
    }
}