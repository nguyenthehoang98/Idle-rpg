#if UNITY_INCLUDE_TESTS
using _GameToolkit.SkillSystem.Core;
using _GameToolkit.SkillSystem.Imp;
using NUnit.Framework;
using UnityEngine;

namespace _GameToolkit.SkillSystem.Tests
{
    public class TrajectoryTests
    {
        [Test]
        public void ProjectileTrajectory_StartsAtStart_EndsAtGoal()
        {
            var start = new Vector2(0f, 0f);
            var goal = new Vector2(10f, 0f);

            var trajectory = new ProjectileTrajectory(
                AnimationCurve.Linear(0f, 0f, 1f, 1f),
                10f, 1f, start, goal);

            // Tại t=0: vị trí = start
            Vector2 p0 = trajectory.EvaluatePosition(0f);
            Assert.AreEqual(start.x, p0.x, 0.001f);

            // Sau 1s: tới goal
            Vector2 p1 = trajectory.EvaluatePosition(1f);
            Assert.AreEqual(goal.x, p1.x, 0.001f);
        }

        [Test]
        public void ProjectileTrajectory_Midway_PositionIsHalf()
        {
            var start = new Vector2(0f, 0f);
            var goal = new Vector2(10f, 0f);

            var trajectory = new ProjectileTrajectory(
                AnimationCurve.Linear(0f, 0f, 1f, 1f),
                10f, 1f, start, goal);

            Vector2 p = trajectory.EvaluatePosition(0.5f);

            Assert.AreEqual(5f, p.x, 0.001f);
        }

        [Test]
        public void ProjectileTrajectory_ZeroDuration_ImmediatelyGoal()
        {
            var start = new Vector2(0f, 0f);
            var goal = new Vector2(5f, 0f);

            var trajectory = new ProjectileTrajectory(
                AnimationCurve.Linear(0f, 0f, 1f, 1f),
                5f, 0f, start, goal);

            Vector2 p = trajectory.EvaluatePosition(0.1f);

            Assert.AreEqual(goal.x, p.x, 0.001f);
        }

        [Test]
        public void StationaryTrajectory_AlwaysReturnsGoal()
        {
            var trajectory = new StationaryTrajectory(Vector2.zero, new Vector2(3f, 4f));

            Assert.AreEqual(new Vector2(3f, 4f), trajectory.EvaluatePosition(0f));
            Assert.AreEqual(new Vector2(3f, 4f), trajectory.EvaluatePosition(5f));
        }

        [Test]
        public void BoomerangTrajectory_Outbound_ThenReturnsToStart()
        {
            var start = new Vector2(0f, 0f);
            var goal = new Vector2(10f, 0f);

            var trajectory = new BoomerangTrajectory(
                AnimationCurve.Linear(0f, 0f, 1f, 1f),
                AnimationCurve.Linear(0f, 0f, 1f, 1f),
                1f, 0f, 1f, start, goal);

            // Outbound: t=0.5 → giữa đường
            Vector2 outbound = trajectory.EvaluatePosition(0.5f);
            Assert.AreEqual(5f, outbound.x, 0.001f);

            // Return: t=1.5 → về start
            Vector2 returned = trajectory.EvaluatePosition(1.5f);
            Assert.AreEqual(start.x, returned.x, 0.001f);
        }

        [Test]
        public void Trajectory_Direction_IsNormalized()
        {
            var start = new Vector2(0f, 0f);
            var goal = new Vector2(0f, 10f);

            var trajectory = new ProjectileTrajectory(
                AnimationCurve.Linear(0f, 0f, 1f, 1f),
                10f, 1f, start, goal);

            Vector2 dir = trajectory.EvaluateDirection(0.1f);

            Assert.AreEqual(0f, dir.x, 0.001f);
            Assert.AreEqual(1f, dir.y, 0.001f);
        }
    }
}
#endif
