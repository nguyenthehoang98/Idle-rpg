using System.Collections.Generic;
using _KITSystem.SkillSystem.Model;
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

        [Test]
        public void SkillId_ShouldBeReused_AfterAllActionsRemoved()
        {
            var spu = new Spu();

            // create skill
            int skill1 = spu.GenerateSkillInstanceId();

            int actionId = -1;

            spu.RequestAddAction(skill1, new DummyAction(), id => actionId = id);
            spu.Tick(0);

            // remove action -> skill should be released
            spu.RequestRemoveAction(actionId);
            spu.Tick(0);

            // create new skill -> expect reuse
            int skill2 = spu.GenerateSkillInstanceId();

            Assert.AreEqual(skill1, skill2);
        }
        
        [Test]
        public void SkillId_ShouldNotBeReused_WhenStillHasActions()
        {
            var spu = new Spu();

            int skill1 = spu.GenerateSkillInstanceId();

            spu.RequestAddAction(skill1, new DummyAction());
            spu.Tick(0);

            int skill2 = spu.GenerateSkillInstanceId();

            Assert.AreNotEqual(skill1, skill2);
        }
        
        [Test]
        public void ActionId_ShouldNotBeReused()
        {
            var spu = new Spu();

            int skill = spu.GenerateSkillInstanceId();

            int a1 = -1;
            int a2 = -1;

            spu.RequestAddAction(skill, new DummyAction(), id => a1 = id);
            spu.Tick(0);

            spu.RequestRemoveAction(a1);
            spu.Tick(0);

            spu.RequestAddAction(skill, new DummyAction(), id => a2 = id);
            spu.Tick(0);

            Assert.AreNotEqual(a1, a2);
        }
        
        [Test]
        public void SkillId_ShouldOnlyBeFreedOnce()
        {
            var spu = new Spu();

            int skill = spu.GenerateSkillInstanceId();

            int a1 = -1, a2 = -1;

            spu.RequestAddAction(skill, new DummyAction(), id => a1 = id);
            spu.RequestAddAction(skill, new DummyAction(), id => a2 = id);
            spu.Tick(0);

            // remove cả 2
            spu.RequestRemoveAction(a1);
            spu.RequestRemoveAction(a2);
            spu.Tick(0);

            // tạo lại 2 skill
            int s1 = spu.GenerateSkillInstanceId();
            int s2 = spu.GenerateSkillInstanceId();

            // chỉ 1 cái được reuse
            Assert.IsTrue(s1 == skill || s2 == skill);
        }
        
        [Test]
        public void SkillId_Reuse_ShouldBeStable_AfterManyCycles()
        {
            var spu = new Spu();

            const int LOOP = 1000;

            for (int i = 0; i < LOOP; i++)
            {
                int skill = spu.GenerateSkillInstanceId();

                int actionId = -1;

                spu.RequestAddAction(skill, new DummyAction(), id => actionId = id);
                spu.Tick(0);

                spu.RequestRemoveAction(actionId);
                spu.Tick(0);

                int reused = spu.GenerateSkillInstanceId();

                Assert.AreEqual(skill, reused, $"Failed at iteration {i}");
            }
        }
        
        [Test]
        public void SkillId_Reuse_MultipleSkills()
        {
            var spu = new Spu();

            const int COUNT = 100;

            var skills = new List<int>();

            // tạo nhiều skill
            for (int i = 0; i < COUNT; i++)
            {
                skills.Add(spu.GenerateSkillInstanceId());
            }

            var actionIds = new List<int>();

            // add action
            foreach (var skill in skills)
            {
                int a = -1;
                spu.RequestAddAction(skill, new DummyAction(), id => a = id);
                spu.Tick(0);
                actionIds.Add(a);
            }

            // remove tất cả
            foreach (var a in actionIds)
            {
                spu.RequestRemoveAction(a);
            }
            spu.Tick(0);

            // tạo lại skill
            var reused = new HashSet<int>();

            for (int i = 0; i < COUNT; i++)
            {
                reused.Add(spu.GenerateSkillInstanceId());
            }

            foreach (var s in skills)
            {
                Assert.IsTrue(reused.Contains(s), $"Skill {s} was not reused");
            }
        }
        
        [Test]
        public void Random_Stress_Test()
        {
            var spu = new Spu();
            var rnd = new System.Random(123);

            var activeActions = new List<int>();
            var activeSkills = new List<int>();

            const int ITER = 5000;

            for (int i = 0; i < ITER; i++)
            {
                int op = rnd.Next(3);

                if (op == 0) // add skill + action
                {
                    int skill = spu.GenerateSkillInstanceId();
                    activeSkills.Add(skill);

                    int actionId = -1;
                    spu.RequestAddAction(skill, new DummyAction(), id => actionId = id);
                    spu.Tick(0);

                    activeActions.Add(actionId);
                }
                else if (op == 1 && activeActions.Count > 0) // remove random action
                {
                    int idx = rnd.Next(activeActions.Count);
                    int action = activeActions[idx];

                    spu.RequestRemoveAction(action);
                    spu.Tick(0);

                    activeActions.RemoveAt(idx);
                }

                // invariant check
                foreach (var a in activeActions)
                {
                    Assert.IsTrue(spu.HasAction(a), $"Missing action {a}");
                }
            }
        }
        
        [Test]
        public void ActionId_ShouldAlwaysBeUnique()
        {
            var spu = new Spu();

            var ids = new HashSet<int>();

            const int ITER = 2000;

            for (int i = 0; i < ITER; i++)
            {
                int skill = spu.GenerateSkillInstanceId();

                int actionId = -1;

                spu.RequestAddAction(skill, new DummyAction(), id => actionId = id);
                spu.Tick(0);

                Assert.IsFalse(ids.Contains(actionId), $"Duplicate actionId {actionId}");

                ids.Add(actionId);

                spu.RequestRemoveAction(actionId);
                spu.Tick(0);
            }
        }
        
        Spu CreateSPU()
        {
            return new Spu();
        }
    }

    //========================
    // DUMMY ACTION
    //========================

    class DummyAction : IAction
    {
        public bool IsFinished => false;
        public ActionCompleteReason Reason => ActionCompleteReason.EndLifeCycle;

        public void Start() { }
        public void Trigger(int id)
        {
        }

        public void Tick(float deltaTime) { }
        public void Stop() { }
        public void Interrupt() { }
    }

    class FinishImmediatelyAction : IAction
    {
        public bool IsFinished => true;
        public ActionCompleteReason Reason => ActionCompleteReason.EndLifeCycle;

        public void Start() { }
        public void Trigger(int id)
        {
        }

        public void Tick(float deltaTime) { }
        public void Stop() { }
        public void Interrupt() { }
    }
}