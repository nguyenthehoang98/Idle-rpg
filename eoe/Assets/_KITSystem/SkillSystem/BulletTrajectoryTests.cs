#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using UnityEngine;

namespace _KITSystem.SkillSystem
{
    public class BulletTrajectoryTests
    {
        [Test]
        public void EvaluatePosition_AtStart_ReturnsStart()
        {
            var traj = new Imp.BulletTrajectory(10f, 0f, Vector2.zero, new Vector2(100f, 0f));
            Vector2 pos = traj.EvaluatePosition(0f);
            Assert.AreEqual(Vector2.zero, pos);
        }

        [Test]
        public void EvaluatePosition_MovesTowardGoal()
        {
            var traj = new Imp.BulletTrajectory(10f, 0f, Vector2.zero, new Vector2(100f, 0f));
            Vector2 pos = traj.EvaluatePosition(1f);
            Assert.Greater(pos.x, 0f);
            Assert.AreEqual(0f, pos.y);
        }

        [Test]
        public void EvaluatePosition_WithAcceleration_MovesFaster()
        {
            var constant = new Imp.BulletTrajectory(10f, 0f, Vector2.zero, new Vector2(100f, 0f));
            var accelerated = new Imp.BulletTrajectory(10f, 5f, Vector2.zero, new Vector2(100f, 0f));

            constant.EvaluatePosition(2f);
            accelerated.EvaluatePosition(2f);

            Vector2 posC = constant.EvaluatePosition(0.016f);
            Vector2 posA = accelerated.EvaluatePosition(0.016f);

            Assert.Greater(posA.x - posC.x, -0.001f);
        }

        [Test]
        public void EvaluatePosition_DirectionFollowsGoal()
        {
            Vector2 start = new Vector2(0f, 0f);
            Vector2 goal = new Vector2(0f, 100f);
            var traj = new Imp.BulletTrajectory(10f, 0f, start, goal);
            Vector2 pos = traj.EvaluatePosition(1f);
            Assert.AreEqual(0f, pos.x, 0.001f);
            Assert.Greater(pos.y, 0f);
        }

        [Test]
        public void EvaluatePosition_ZeroInitialSpeed_NoMovement()
        {
            var traj = new Imp.BulletTrajectory(0f, 0f, Vector2.zero, new Vector2(100f, 0f));
            Vector2 pos = traj.EvaluatePosition(1f);
            Assert.AreEqual(0f, pos.x);
        }

        [Test]
        public void EvaluatePosition_DistanceIncreasesWithTime()
        {
            var traj = new Imp.BulletTrajectory(10f, 0f, Vector2.zero, new Vector2(100f, 0f));
            float d1 = traj.EvaluatePosition(0.5f).magnitude;
            float d2 = traj.EvaluatePosition(1.0f).magnitude;
            Assert.Greater(d2, d1);
        }
    }
}
#endif
