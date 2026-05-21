using System.Collections.Generic;
using _KITSystem.SkillSystem.Entity;
using NUnit.Framework;

namespace _KITSystem.SkillSystem.Unitest
{
    public class EntityManagerTesting
    {
        [SetUp]
        public void Setup()
        {
            EntityManager.Clear();
        }

        #region Entity Lifecycle

        [Test]
        public void NewEntity_ShouldReturnValidId()
        {
            int entity = EntityManager.CreateEntity();

            Assert.IsTrue(entity > 0);
            Assert.IsTrue(EntityManager.IsAlive(entity));
            Assert.AreEqual(1, EntityManager.ActiveCount);
        }

        [Test]
        public void NewEntity_ShouldIncrementId()
        {
            int e1 = EntityManager.CreateEntity();
            int e2 = EntityManager.CreateEntity();
            int e3 = EntityManager.CreateEntity();

            Assert.AreNotEqual(e1, e2);
            Assert.AreNotEqual(e2, e3);
            Assert.AreEqual(3, EntityManager.ActiveCount);
        }

        [Test]
        public void DestroyEntity_ShouldMarkDead()
        {
            int entity = EntityManager.CreateEntity();
            EntityManager.DestroyEntity(entity);

            Assert.IsFalse(EntityManager.IsAlive(entity));
            Assert.AreEqual(0, EntityManager.ActiveCount);
        }

        [Test]
        public void DestroyEntity_ShouldFreeIdForReuse()
        {
            int e1 = EntityManager.CreateEntity();
            EntityManager.DestroyEntity(e1);

            int e2 = EntityManager.CreateEntity();

            Assert.AreEqual(e1, e2);
            Assert.AreEqual(1, EntityManager.ActiveCount);
        }

        [Test]
        public void DestroyEntity_Invalid_ShouldNotCrash()
        {
            Assert.DoesNotThrow(() => EntityManager.DestroyEntity(0));
            Assert.DoesNotThrow(() => EntityManager.DestroyEntity(99999));
        }

        [Test]
        public void DestroyEntity_DoubleDestroy_ShouldNotCrash()
        {
            int entity = EntityManager.CreateEntity();
            EntityManager.DestroyEntity(entity);
            EntityManager.DestroyEntity(entity);

            Assert.AreEqual(0, EntityManager.ActiveCount);
        }

        [Test]
        public void Clear_ShouldResetAll()
        {
            EntityManager.CreateEntity();
            EntityManager.CreateEntity();
            EntityManager.CreateEntity();

            EntityManager.Clear();

            Assert.AreEqual(0, EntityManager.ActiveCount);
            Assert.IsFalse(EntityManager.IsAlive(1));
        }

        [Test]
        public void StressTest_EntityCreateDestroy()
        {
            const int ITER = 10000;

            for (int i = 0; i < ITER; i++)
            {
                int entity = EntityManager.CreateEntity();
                Assert.IsTrue(EntityManager.IsAlive(entity));
                EntityManager.DestroyEntity(entity);
                Assert.IsFalse(EntityManager.IsAlive(entity));
            }

            Assert.AreEqual(0, EntityManager.ActiveCount);
        }

        #endregion

        #region Component Operations

        [Test]
        public void AddComponent_ShouldBeRetrievable()
        {
            int entity = EntityManager.CreateEntity();
            EntityManager.AddComponent(entity, new HealthComponent { Current = 100, Max = 100 });

            Assert.IsTrue(EntityManager.HasComponent<HealthComponent>(entity));

            ref var health = ref EntityManager.GetComponent<HealthComponent>(entity);
            Assert.AreEqual(100, health.Current);
        }

        [Test]
        public void AddComponent_ShouldModifyByRef()
        {
            int entity = EntityManager.CreateEntity();
            EntityManager.AddComponent(entity, new HealthComponent { Current = 100, Max = 100 });

            ref var health = ref EntityManager.GetComponent<HealthComponent>(entity);
            health.Current = 50;

            ref var health2 = ref EntityManager.GetComponent<HealthComponent>(entity);
            Assert.AreEqual(50, health2.Current);
        }

        [Test]
        public void TryGetComponent_ShouldReturnFalse_WhenMissing()
        {
            int entity = EntityManager.CreateEntity();

            bool result = EntityManager.TryGetComponent<HealthComponent>(entity, out _);

            Assert.IsFalse(result);
        }

        [Test]
        public void TryGetComponent_ShouldReturnTrue_WhenPresent()
        {
            int entity = EntityManager.CreateEntity();
            EntityManager.AddComponent(entity, new HealthComponent { Current = 50 });

            bool result = EntityManager.TryGetComponent<HealthComponent>(entity, out var health);

            Assert.IsTrue(result);
            Assert.AreEqual(50, health.Current);
        }

        [Test]
        public void RemoveComponent_ShouldRemove()
        {
            int entity = EntityManager.CreateEntity();
            EntityManager.AddComponent(entity, new HealthComponent());

            EntityManager.RemoveComponent<HealthComponent>(entity);

            Assert.IsFalse(EntityManager.HasComponent<HealthComponent>(entity));
        }

        [Test]
        public void RemoveComponent_Missing_ShouldNotCrash()
        {
            int entity = EntityManager.CreateEntity();

            Assert.DoesNotThrow(() => EntityManager.RemoveComponent<HealthComponent>(entity));
        }

        [Test]
        public void AddComponent_Duplicate_ShouldThrow()
        {
            int entity = EntityManager.CreateEntity();
            EntityManager.AddComponent(entity, new HealthComponent());

            Assert.Throws<System.InvalidOperationException>(() =>
                EntityManager.AddComponent(entity, new HealthComponent()));
        }

        [Test]
        public void AddComponent_DeadEntity_ShouldThrow()
        {
            int entity = EntityManager.CreateEntity();
            EntityManager.DestroyEntity(entity);

            Assert.Throws<System.ArgumentException>(() =>
                EntityManager.AddComponent(entity, new HealthComponent()));
        }

        #endregion

        #region Query

        [Test]
        public void Query_SingleComponent_ShouldReturnAllWithComponent()
        {
            int e1 = EntityManager.CreateEntity();
            int e2 = EntityManager.CreateEntity();
            int e3 = EntityManager.CreateEntity();

            EntityManager.AddComponent(e1, new HealthComponent { Current = 10 });
            EntityManager.AddComponent(e2, new HealthComponent { Current = 20 });

            var results = new List<int>();
            foreach (var id in EntityManager.Query<HealthComponent>())
                results.Add(id);

            Assert.AreEqual(2, results.Count);
            Assert.Contains(e1, results);
            Assert.Contains(e2, results);
            Assert.IsFalse(results.Contains(e3));
        }

        [Test]
        public void Query_TwoComponents_ShouldReturnIntersection()
        {
            int e1 = EntityManager.CreateEntity();
            int e2 = EntityManager.CreateEntity();
            int e3 = EntityManager.CreateEntity();

            EntityManager.AddComponent(e1, new HealthComponent());
            EntityManager.AddComponent(e1, new PositionComponent());

            EntityManager.AddComponent(e2, new HealthComponent());

            EntityManager.AddComponent(e3, new PositionComponent());

            var results = new List<int>();
            foreach (var id in EntityManager.Query<HealthComponent, PositionComponent>())
                results.Add(id);

            Assert.AreEqual(1, results.Count);
            Assert.Contains(e1, results);
        }

        [Test]
        public void Query_ThreeComponents_ShouldReturnIntersection()
        {
            int e1 = EntityManager.CreateEntity();
            int e2 = EntityManager.CreateEntity();

            EntityManager.AddComponent(e1, new HealthComponent());
            EntityManager.AddComponent(e1, new PositionComponent());
            EntityManager.AddComponent(e1, new VelocityComponent());

            EntityManager.AddComponent(e2, new HealthComponent());
            EntityManager.AddComponent(e2, new PositionComponent());

            var results = new List<int>();
            foreach (var id in EntityManager.Query<HealthComponent, PositionComponent, VelocityComponent>())
                results.Add(id);

            Assert.AreEqual(1, results.Count);
            Assert.Contains(e1, results);
            Assert.IsFalse(results.Contains(e2));
        }

        [Test]
        public void Query_AfterDestroy_ShouldNotReturnDeadEntity()
        {
            int e1 = EntityManager.CreateEntity();
            int e2 = EntityManager.CreateEntity();

            EntityManager.AddComponent(e1, new HealthComponent());
            EntityManager.AddComponent(e2, new HealthComponent());

            EntityManager.DestroyEntity(e1);

            var results = new List<int>();
            foreach (var id in EntityManager.Query<HealthComponent>())
                results.Add(id);

            Assert.AreEqual(1, results.Count);
            Assert.Contains(e2, results);
        }

        [Test]
        public void Query_AfterRemoveComponent_ShouldNotReturnEntity()
        {
            int entity = EntityManager.CreateEntity();
            EntityManager.AddComponent(entity, new HealthComponent());

            EntityManager.RemoveComponent<HealthComponent>(entity);

            var results = new List<int>();
            foreach (var id in EntityManager.Query<HealthComponent>())
                results.Add(id);

            Assert.AreEqual(0, results.Count);
        }

        #endregion

        #region DestroyEntity Cleans Components

        [Test]
        public void DestroyEntity_ShouldRemoveAllComponents()
        {
            int entity = EntityManager.CreateEntity();
            EntityManager.AddComponent(entity, new HealthComponent());
            EntityManager.AddComponent(entity, new PositionComponent());
            EntityManager.AddComponent(entity, new VelocityComponent());

            EntityManager.DestroyEntity(entity);

            Assert.IsFalse(EntityManager.HasComponent<HealthComponent>(entity));
            Assert.IsFalse(EntityManager.HasComponent<PositionComponent>(entity));
            Assert.IsFalse(EntityManager.HasComponent<VelocityComponent>(entity));
        }

        [Test]
        public void DestroyEntity_ThenReuseId_ShouldBeClean()
        {
            int e1 = EntityManager.CreateEntity();
            EntityManager.AddComponent(e1, new HealthComponent { Current = 999 });
            EntityManager.DestroyEntity(e1);

            int e2 = EntityManager.CreateEntity();
            Assert.AreEqual(e1, e2);

            Assert.IsFalse(EntityManager.HasComponent<HealthComponent>(e2));
        }

        #endregion

        #region Stress Tests

        [Test]
        public void StressTest_ComponentAddRemove()
        {
            const int ITER = 5000;
            var entities = new int[100];

            for (int i = 0; i < 100; i++)
                entities[i] = EntityManager.CreateEntity();

            var rnd = new System.Random(42);

            for (int i = 0; i < ITER; i++)
            {
                int idx = rnd.Next(100);
                int entity = entities[idx];
                int op = rnd.Next(4);
                if (op == 0 && !EntityManager.HasComponent<HealthComponent>(entity))
                    EntityManager.AddComponent(entity, new HealthComponent { Current = rnd.Next(100) });
                else if (op == 1 && !EntityManager.HasComponent<PositionComponent>(entity))
                    EntityManager.AddComponent(entity, new PositionComponent { X = rnd.Next(), Y = rnd.Next() });
                else if (op == 2 && EntityManager.HasComponent<HealthComponent>(entity))
                    EntityManager.RemoveComponent<HealthComponent>(entity);
                else if (op == 3 && EntityManager.HasComponent<PositionComponent>(entity))
                    EntityManager.RemoveComponent<PositionComponent>(entity);
            }
        }

        [Test]
        public void StressTest_MixedOperations()
        {
            const int ITER = 2000;
            var active = new List<int>();
            var rnd = new System.Random(123);

            for (int i = 0; i < ITER; i++)
            {
                int op = rnd.Next(4);

                if (op == 0)
                {
                    int entity = EntityManager.CreateEntity();
                    EntityManager.AddComponent(entity, new HealthComponent { Current = rnd.Next(100) });
                    EntityManager.AddComponent(entity, new PositionComponent());
                    active.Add(entity);
                }
                else if (op == 1 && active.Count > 0)
                {
                    int idx = rnd.Next(active.Count);
                    EntityManager.DestroyEntity(active[idx]);
                    active.RemoveAt(idx);
                }
                else if (op == 2 && active.Count > 0)
                {
                    int idx = rnd.Next(active.Count);
                    EntityManager.RemoveComponent<HealthComponent>(active[idx]);
                }
                else if (op == 3)
                {
                    foreach (var id in EntityManager.Query<HealthComponent, PositionComponent>())
                    {
                        ref var health = ref EntityManager.GetComponent<HealthComponent>(id);
                        health.Current--;
                    }
                }
            }
        }

        #endregion
    }

    // Test components
    internal struct HealthComponent
    {
        internal int Current;
        internal int Max;
    }

    internal struct PositionComponent
    {
        internal float X;
        internal float Y;
    }

    internal struct VelocityComponent
    {
        internal float VX;
        internal float VY;
    }
}
