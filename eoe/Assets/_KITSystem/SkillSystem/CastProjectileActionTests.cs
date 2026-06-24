#if UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using _Game.GamePlay;
using _Game.GamePlay.Model;
using _KITSystem.SkillSystem.Core;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace _KITSystem.SkillSystem
{
    public class CastProjectileActionTests
    {
        private class MockCollider : BaseCollider
        {
            private readonly List<int> fakeResults;
            private float time;

            public MockCollider(List<int> results, float timerTrigger, float duration)
                : base(null, Vector2.zero, timerTrigger, duration)
            {
                fakeResults = results;
            }

            protected override List<int> OnCollision(Vector2 position)
            {
                return fakeResults;
            }

            public new void Tick(float deltaTime)
            {
                time += deltaTime;
                base.Tick(deltaTime);
            }

            public new List<int> Collision(Vector2 position)
            {
                return base.Collision(position);
            }
        }

        private class MockTrajectory : BaseTrajectory
        {
            public MockTrajectory() : base(Vector2.zero, Vector2.one * 100f) { }

            protected override Vector2 OnEvaluatePosition(float deltaTime)
            {
                return Start + Direction * 10f;
            }
        }

        private static Projectile CreateDummyProjectile()
        {
            return new GameObject("TestProjectile").AddComponent<Projectile>();
        }

        [Test]
        public void Tick_WithCollision_CallsDamageCallback()
        {
            bool damaged = false;
            var spu = new Spu();
            var collider = new MockCollider(new List<int> { 1 }, 0f, 10f);
            var trajectory = new MockTrajectory();
            var projectile = CreateDummyProjectile();

            var action = new Imp.CastProjectileAction(spu, 10f, collider, trajectory,
                (entity, pos) => { damaged = true; return true; },
                projectile, DamageTickerType.Instant, 0f, 10, 0f);

            action.Start();
            collider.Tick(0.1f);
            action.Tick(1f);

            Object.DestroyImmediate(projectile.gameObject);
            Assert.IsTrue(damaged);
        }

        [Test]
        public void Tick_LimitCollisions_EndsLifeCycle()
        {
            int hits = 0;
            var spu = new Spu();
            var collider = new MockCollider(new List<int> { 1 }, 0f, 10f);
            var trajectory = new MockTrajectory();
            var projectile = CreateDummyProjectile();

            var action = new Imp.CastProjectileAction(spu, 10f, collider, trajectory,
                (entity, pos) => { hits++; return true; },
                projectile, DamageTickerType.Instant, 0f, 1, 0f);

            action.Start();
            collider.Tick(0.1f);
            action.Tick(1f);

            Object.DestroyImmediate(projectile.gameObject);
            Assert.AreEqual(1, hits);
            Assert.IsTrue(action.IsFinished);
        }

        [Test]
        public void Stop_InvokesOnComplete()
        {
            bool completed = false;
            var spu = new Spu();
            var collider = new MockCollider(new List<int>(), 0f, 10f);
            var trajectory = new MockTrajectory();
            var projectile = CreateDummyProjectile();

            var action = new Imp.CastProjectileAction(spu, 10f, collider, trajectory,
                (entity, pos) => true, projectile, DamageTickerType.Instant, 0f, 10, 0f);

            action.OnComplete += () => completed = true;
            action.Start();
            action.Stop();

            Object.DestroyImmediate(projectile.gameObject);
            Assert.IsTrue(completed);
        }

        [Test]
        public void Tick_MultipleCollisions_TracksCorrectCount()
        {
            int hits = 0;
            var spu = new Spu();
            var collider = new MockCollider(new List<int> { 1, 2, 3 }, 0f, 10f);
            var trajectory = new MockTrajectory();
            var projectile = CreateDummyProjectile();

            var action = new Imp.CastProjectileAction(spu, 10f, collider, trajectory,
                (entity, pos) => { hits++; return true; },
                projectile, DamageTickerType.Instant, 0f, 2, 0f);

            action.Start();
            collider.Tick(0.1f);
            action.Tick(1f);

            Object.DestroyImmediate(projectile.gameObject);
            Assert.AreEqual(2, hits);
            Assert.IsTrue(action.IsFinished);
        }
    }
}
#endif
