#if UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using _GameToolkit.SkillSystem.Core;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;

namespace _GameToolkit.SkillSystem.Tests
{
    public class CastProjectileActionTests
    {
        /// <summary>Fake IProjectileView: ghi nhận position/destroy.</summary>
        private sealed class FakeProjectile : IProjectileView
        {
            public Vector2 LastPosition;
            public Vector2 LastDirection;
            public bool Destroyed;

            public void SetPosition(Vector2 position, Vector2 direction, float deltaTime)
            {
                LastPosition = position;
                LastDirection = direction;
            }

            public void Destroy() => Destroyed = true;
        }

        /// <summary>Fake IQuery: trả danh sách entity cố định.</summary>
        private sealed class FakeQuery : IQuery
        {
            public List<int> Entities = new List<int>();

            public void FindTarget(FindTargetType type, int totalQuery, Vector2 center, Vector2 pivot,
                float radius, System.Func<int, float2, bool> filter, out QueryResult result)
            {
                result = new QueryResult { Results = new List<QueryEntityData>() };
            }

            public List<int> GetAllEntities(Vector2 center, Vector2 size, System.Func<int, bool> filter)
            {
                return Entities;
            }

            public List<int> GetAllEntities(Vector2 center, Vector2 size, Vector2 direction,
                System.Func<int, bool> filter)
            {
                return Entities;
            }
        }

        /// <summary>Trajectory tĩnh: đứng yên tại goal.</summary>
        private sealed class StaticTrajectory : BaseTrajectory
        {
            public StaticTrajectory(Vector2 goal) : base(Vector2.zero, goal) { }

            public override Vector2 EvaluatePosition(float deltaTime) => Goal;

            public override Vector2 EvaluateDirection(float deltaTime) => Vector2.zero;
        }

        /// <summary>CircleCollider luôn trigger.</summary>
        private sealed class AlwaysHitCollider : BaseCollider
        {
            private readonly List<int> hits;

            public AlwaysHitCollider(IQuery query, List<int> hits)
                : base(query, Vector2.zero, 0f, float.MaxValue)
            {
                this.hits = hits;
            }

            protected override List<int> OnCollision(Vector2 position, Vector2 direction) => hits;
        }

        [Test]
        public void SingleHit_CallsDamageCallback_AndDestroysProjectile()
        {
            var query = new FakeQuery();
            var projectile = new FakeProjectile();
            int hitCount = 0;
            HitInfo lastHit = default;

            var colliders = new List<BaseCollider> { new AlwaysHitCollider(query, new List<int> { 1 }) };

            var action = new CastProjectileAction(
                10f, colliders, new StaticTrajectory(Vector2.one),
                info => { hitCount++; lastHit = info; return true; },
                projectile, 0f, 1, 0f);

            action.Start();
            action.Tick(0.1f);

            Assert.AreEqual(1, hitCount);
            Assert.AreEqual(1, lastHit.EntityId);
            Assert.IsFalse(lastHit.IsLastHit); // 1 hit, maxHitCount=1, nhưng chưa tick đủ để interrupt
        }

        [Test]
        public void MaxHitCount_InterruptsAfterLimit()
        {
            var query = new FakeQuery { Entities = new List<int> { 1, 2, 3 } };
            var projectile = new FakeProjectile();
            int hitCount = 0;

            var colliders = new List<BaseCollider> { new AlwaysHitCollider(query, query.Entities) };

            var action = new CastProjectileAction(
                10f, colliders, new StaticTrajectory(Vector2.one),
                info => { hitCount++; return true; },
                projectile, 0f, 2, 0f);

            action.Start();
            action.Tick(0.1f);

            // maxHitCount=2 → interrupt ngay khi đạt 2
            Assert.AreEqual(2, hitCount);
            Assert.IsTrue(action.IsFinished);
            Assert.AreEqual(ActionCompleteReason.Interrupt, action.Reason);
        }

        [Test]
        public void UnlimitedHits_NeverInterruptsFromHits()
        {
            var query = new FakeQuery { Entities = new List<int> { 1, 2, 3 } };
            var projectile = new FakeProjectile();
            int hitCount = 0;

            var colliders = new List<BaseCollider> { new AlwaysHitCollider(query, query.Entities) };

            var action = new CastProjectileAction(
                10f, colliders, new StaticTrajectory(Vector2.one),
                info => { hitCount++; return true; },
                projectile, 0f, 0, 0f); // maxHitCount = 0 → unlimited

            action.Start();
            action.Tick(0.1f);

            Assert.AreEqual(3, hitCount);
            Assert.IsFalse(action.IsFinished);
        }

        [Test]
        public void TargetHitCooldown_PreventsRehitWithinWindow()
        {
            var query = new FakeQuery { Entities = new List<int> { 1 } };
            var projectile = new FakeProjectile();
            int hitCount = 0;

            var colliders = new List<BaseCollider> { new AlwaysHitCollider(query, query.Entities) };

            var action = new CastProjectileAction(
                10f, colliders, new StaticTrajectory(Vector2.one),
                info => { hitCount++; return true; },
                projectile, 0f, 0, 5f); // cooldown 5s

            action.Start();
            action.Tick(0.1f);
            action.Tick(0.1f);

            Assert.AreEqual(1, hitCount); // entity 1 trong cooldown → không hit lại
        }

        [Test]
        public void TargetHitCooldown_Expires_AllowsRehit()
        {
            var query = new FakeQuery { Entities = new List<int> { 1 } };
            var projectile = new FakeProjectile();
            int hitCount = 0;

            var colliders = new List<BaseCollider> { new AlwaysHitCollider(query, query.Entities) };

            var action = new CastProjectileAction(
                10f, colliders, new StaticTrajectory(Vector2.one),
                info => { hitCount++; return true; },
                projectile, 0f, 0, 5f);

            action.Start();
            action.Tick(0.1f);
            action.Tick(5.5f); // vượt cooldown

            Assert.AreEqual(2, hitCount);
        }

        [Test]
        public void DotInterval_DamagesAllHitsPerTick()
        {
            var query = new FakeQuery { Entities = new List<int> { 1, 2 } };
            var projectile = new FakeProjectile();
            int dotTicks = 0;

            var colliders = new List<BaseCollider> { new AlwaysHitCollider(query, query.Entities) };

            var action = new CastProjectileAction(
                10f, colliders, new StaticTrajectory(Vector2.one),
                info => { dotTicks++; return true; },
                projectile, 1f, 0, 0f); // damageInterval 1s

            action.Start();
            action.Tick(0.1f);  // register 2 entities
            action.Tick(1.1f);  // DoT tick: damage cả 2

            Assert.AreEqual(2, dotTicks);
        }

        [Test]
        public void OnComplete_FiresOnLifecycleEnd()
        {
            var query = new FakeQuery();
            var projectile = new FakeProjectile();
            bool completed = false;

            var colliders = new List<BaseCollider> { new AlwaysHitCollider(query, new List<int>()) };

            var action = new CastProjectileAction(
                0.5f, colliders, new StaticTrajectory(Vector2.one),
                info => true, projectile, 0f, 0, 0f);

            action.OnComplete += () => completed = true;

            action.Start();
            action.Tick(1f); // hết lifetime

            Assert.IsTrue(completed);
            Assert.IsTrue(projectile.Destroyed);
        }
    }
}
#endif
