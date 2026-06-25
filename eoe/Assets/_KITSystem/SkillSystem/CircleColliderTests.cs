#if UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using _KITSystem.SkillSystem.Core;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;

namespace _KITSystem.SkillSystem
{
    public class CircleColliderTests
    {
        private class MockQuery : IQuery
        {
            private readonly List<int> entities;
            private readonly Vector2 center;
            private readonly Vector2 size;

            public MockQuery(List<int> entities, Vector2 center, Vector2 size)
            {
                this.entities = entities;
                this.center = center;
                this.size = size;
            }

            public void FindTarget(FindTargetType type, Vector2 center, Vector2 pivot, float radius, Func<int, float2, bool> funcFilterEntity,
                out QueryResult result)
            {
                result = new QueryResult();
            }

            public List<int> GetAllEntities(Vector2 queryCenter, Vector2 querySize, Func<int, bool> funcFilterEntity)
            {
                if (queryCenter == center && querySize == size)
                    return entities;
                return new List<int>();
            }
        }

        [Test]
        public void Collision_WithinTriggerWindow_ReturnsEntities()
        {
            var query = new MockQuery(new List<int> { 1, 2 }, Vector2.zero, new Vector2(1f, 1f));
            var collider = new Imp.CircleCollider(query, Vector2.zero, 0f, 10f, 2f);

            collider.Tick(0.1f);
            var results = collider.Collision(Vector2.zero);

            Assert.IsNotNull(results);
            Assert.AreEqual(2, results.Count);
        }

        [Test]
        public void Collision_BeforeTrigger_ReturnsNull()
        {
            var query = new MockQuery(new List<int> { 1 }, Vector2.zero, new Vector2(1f, 1f));
            var collider = new Imp.CircleCollider(query, Vector2.zero, 5f, 10f, 2f);

            collider.Tick(1f);
            var results = collider.Collision(Vector2.zero);

            Assert.IsNull(results);
        }

        [Test]
        public void Collision_AfterDuration_ReturnsNull()
        {
            var query = new MockQuery(new List<int> { 1 }, Vector2.zero, new Vector2(1f, 1f));
            var collider = new Imp.CircleCollider(query, Vector2.zero, 0f, 1f, 2f);

            collider.Tick(2f);
            var results = collider.Collision(Vector2.zero);

            Assert.IsNull(results);
        }

        [Test]
        public void Collision_NoEntities_ReturnsEmptyList()
        {
            var query = new MockQuery(new List<int>(), Vector2.zero, new Vector2(1f, 1f));
            var collider = new Imp.CircleCollider(query, Vector2.zero, 0f, 10f, 2f);

            collider.Tick(0.1f);
            var results = collider.Collision(Vector2.zero);

            Assert.IsNotNull(results);
            Assert.AreEqual(0, results.Count);
        }

        [Test]
        public void Collision_RelativePosition_QueriesAtOffset()
        {
            Vector2 expectedCenter = new Vector2(5f, 5f);
            var query = new MockQuery(new List<int> { 1 }, expectedCenter, new Vector2(1f, 1f));
            var collider = new Imp.CircleCollider(query, new Vector2(5f, 5f), 0f, 10f, 2f);

            collider.Tick(0.1f);
            var results = collider.Collision(Vector2.zero);

            Assert.IsNotNull(results);
        }

        [Test]
        public void Tick_RespectsTriggerTimer()
        {
            var query = new MockQuery(new List<int> { 1 }, Vector2.zero, new Vector2(1f, 1f));
            var collider = new Imp.CircleCollider(query, Vector2.zero, 3f, 5f, 2f);

            collider.Tick(2.9f);
            Assert.IsNull(collider.Collision(Vector2.zero));

            collider.Tick(0.2f);
            Assert.IsNotNull(collider.Collision(Vector2.zero));
        }
    }
}
#endif
