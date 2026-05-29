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
            EntityManager.Dispose();
        }

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
            EntityManager.Dispose();

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
    }

    public class ComponentManagerTesting
    {
        [SetUp]
        public void Setup()
        {
            ComponentManager<HealthComponent>.Clear();
            ComponentManager<PositionComponent>.Clear();
            ComponentManager<VelocityComponent>.Clear();
        }

        #region Add/Get/Has

        [Test]
        public void Add_ShouldBeRetrievable()
        {
            ComponentManager<HealthComponent>.Add(1, new HealthComponent { Current = 100, Max = 100 });

            Assert.IsTrue(ComponentManager<HealthComponent>.Has(1));
            ref var health = ref ComponentManager<HealthComponent>.Get(1);
            Assert.AreEqual(100, health.Current);
        }

        [Test]
        public void Add_ShouldModifyByRef()
        {
            ComponentManager<HealthComponent>.Add(1, new HealthComponent { Current = 100 });

            ref var health = ref ComponentManager<HealthComponent>.Get(1);
            health.Current = 50;

            ref var health2 = ref ComponentManager<HealthComponent>.Get(1);
            Assert.AreEqual(50, health2.Current);
        }

        [Test]
        public void TryGet_ShouldReturnFalse_WhenMissing()
        {
            bool result = ComponentManager<HealthComponent>.TryGet(1, out _);
            Assert.IsFalse(result);
        }

        [Test]
        public void TryGet_ShouldReturnTrue_WhenPresent()
        {
            ComponentManager<HealthComponent>.Add(1, new HealthComponent { Current = 50 });
            bool result = ComponentManager<HealthComponent>.TryGet(1, out var health);

            Assert.IsTrue(result);
            Assert.AreEqual(50, health.Current);
        }

        [Test]
        public void Has_ShouldReturnFalse_WhenMissing()
        {
            Assert.IsFalse(ComponentManager<HealthComponent>.Has(1));
        }

        [Test]
        public void Has_ShouldReturnTrue_WhenPresent()
        {
            ComponentManager<HealthComponent>.Add(1, new HealthComponent());
            Assert.IsTrue(ComponentManager<HealthComponent>.Has(1));
        }

        #endregion

        #region Remove

        [Test]
        public void Remove_ShouldRemove()
        {
            ComponentManager<HealthComponent>.Add(1, new HealthComponent());
            ComponentManager<HealthComponent>.Remove(1);

            Assert.IsFalse(ComponentManager<HealthComponent>.Has(1));
        }

        [Test]
        public void Remove_Missing_ShouldNotCrash()
        {
            Assert.DoesNotThrow(() => ComponentManager<HealthComponent>.Remove(1));
        }

        [Test]
        public void Remove_ShouldSwapAndPop()
        {
            ComponentManager<HealthComponent>.Add(1, new HealthComponent { Current = 1 });
            ComponentManager<HealthComponent>.Add(2, new HealthComponent { Current = 2 });
            ComponentManager<HealthComponent>.Add(3, new HealthComponent { Current = 3 });

            ComponentManager<HealthComponent>.Remove(2);

            Assert.IsFalse(ComponentManager<HealthComponent>.Has(2));
            Assert.IsTrue(ComponentManager<HealthComponent>.Has(1));
            Assert.IsTrue(ComponentManager<HealthComponent>.Has(3));
            Assert.AreEqual(2, ComponentManager<HealthComponent>.Count);
        }

        #endregion

        #region EntityIds

        [Test]
        public void EntityIds_ShouldReturnAllWithComponent()
        {
            ComponentManager<HealthComponent>.Add(5, new HealthComponent());
            ComponentManager<HealthComponent>.Add(8, new HealthComponent());
            ComponentManager<HealthComponent>.Add(12, new HealthComponent());

            var ids = new List<int>();
            foreach (var id in ComponentManager<HealthComponent>.EntityIds)
                ids.Add(id);

            Assert.AreEqual(3, ids.Count);
            Assert.Contains(5, ids);
            Assert.Contains(8, ids);
            Assert.Contains(12, ids);
        }

        [Test]
        public void EntityIds_ShouldNotContainRemoved()
        {
            ComponentManager<HealthComponent>.Add(1, new HealthComponent());
            ComponentManager<HealthComponent>.Add(2, new HealthComponent());

            ComponentManager<HealthComponent>.Remove(1);

            var ids = new List<int>();
            foreach (var id in ComponentManager<HealthComponent>.EntityIds)
                ids.Add(id);

            Assert.AreEqual(1, ids.Count);
            Assert.Contains(2, ids);
            Assert.IsFalse(ids.Contains(1));
        }

        #endregion

        #region Clear

        [Test]
        public void Clear_ShouldRemoveAll()
        {
            ComponentManager<HealthComponent>.Add(1, new HealthComponent());
            ComponentManager<HealthComponent>.Add(2, new HealthComponent());
            ComponentManager<HealthComponent>.Add(3, new HealthComponent());

            ComponentManager<HealthComponent>.Clear();

            Assert.AreEqual(0, ComponentManager<HealthComponent>.Count);
            Assert.IsFalse(ComponentManager<HealthComponent>.Has(1));
            Assert.IsFalse(ComponentManager<HealthComponent>.Has(2));
            Assert.IsFalse(ComponentManager<HealthComponent>.Has(3));
        }

        [Test]
        public void Clear_ShouldResetEntityMap()
        {
            ComponentManager<HealthComponent>.Add(100, new HealthComponent());
            ComponentManager<HealthComponent>.Clear();

            ComponentManager<HealthComponent>.Add(100, new HealthComponent { Current = 50 });
            Assert.AreEqual(50, ComponentManager<HealthComponent>.Get(100).Current);
        }

        #endregion

        #region Resize

        [Test]
        public void Add_ShouldAutoResize_Past256()
        {
            for (int i = 1; i <= 300; i++)
            {
                ComponentManager<HealthComponent>.Add(i, new HealthComponent { Current = i });
            }

            Assert.AreEqual(300, ComponentManager<HealthComponent>.Count);
            Assert.AreEqual(300, ComponentManager<HealthComponent>.Get(300).Current);
        }

        #endregion

        #region Multiple Types

        [Test]
        public void MultipleComponentTypes_ShouldBeIndependent()
        {
            ComponentManager<HealthComponent>.Add(1, new HealthComponent { Current = 100 });
            ComponentManager<PositionComponent>.Add(1, new PositionComponent { X = 10, Y = 20 });

            Assert.IsTrue(ComponentManager<HealthComponent>.Has(1));
            Assert.IsTrue(ComponentManager<PositionComponent>.Has(1));

            ComponentManager<HealthComponent>.Remove(1);

            Assert.IsFalse(ComponentManager<HealthComponent>.Has(1));
            Assert.IsTrue(ComponentManager<PositionComponent>.Has(1));
        }

        #endregion

        #region Stress Tests

        [Test]
        public void StressTest_AddRemove()
        {
            const int ITER = 5000;
            var entities = new int[100];
            for (int i = 0; i < 100; i++)
                entities[i] = i + 1;

            var rnd = new System.Random(42);

            for (int i = 0; i < ITER; i++)
            {
                int idx = rnd.Next(100);
                int entity = entities[idx];
                int op = rnd.Next(4);

                if (op == 0 && !ComponentManager<HealthComponent>.Has(entity))
                    ComponentManager<HealthComponent>.Add(entity, new HealthComponent { Current = rnd.Next(100) });
                else if (op == 1 && !ComponentManager<PositionComponent>.Has(entity))
                    ComponentManager<PositionComponent>.Add(entity, new PositionComponent { X = rnd.Next(), Y = rnd.Next() });
                else if (op == 2 && ComponentManager<HealthComponent>.Has(entity))
                    ComponentManager<HealthComponent>.Remove(entity);
                else if (op == 3 && ComponentManager<PositionComponent>.Has(entity))
                    ComponentManager<PositionComponent>.Remove(entity);
            }
        }

        [Test]
        public void StressTest_LargeEntityIds()
        {
            ComponentManager<HealthComponent>.Add(5000, new HealthComponent { Current = 99 });
            ComponentManager<HealthComponent>.Add(10000, new HealthComponent { Current = 100 });

            Assert.AreEqual(99, ComponentManager<HealthComponent>.Get(5000).Current);
            Assert.AreEqual(100, ComponentManager<HealthComponent>.Get(10000).Current);

            ComponentManager<HealthComponent>.Remove(5000);
            Assert.IsFalse(ComponentManager<HealthComponent>.Has(5000));
            Assert.IsTrue(ComponentManager<HealthComponent>.Has(10000));
        }

        [Test]
        public void StressTest_RandomOperations()
        {
            const int ITER = 10000;
            var rnd = new System.Random(123);

            for (int i = 0; i < ITER; i++)
            {
                int entity = rnd.Next(1, 500);
                int op = rnd.Next(3);

                if (op == 0)
                    ComponentManager<HealthComponent>.Add(entity, new HealthComponent { Current = rnd.Next() });
                else if (op == 1)
                    ComponentManager<HealthComponent>.Remove(entity);
                else if (op == 2 && ComponentManager<HealthComponent>.Has(entity))
                    _ = ComponentManager<HealthComponent>.Get(entity);
            }
        }

        #endregion
    }

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
