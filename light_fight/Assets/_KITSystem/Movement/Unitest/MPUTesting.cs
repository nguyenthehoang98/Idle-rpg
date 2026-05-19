using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;

namespace _KITSystem.Movement.Unitest
{
    public class MPUTesting
    {
        private MPU mpu;

        [SetUp]
        public void Setup()
        {
            mpu = new MPU(ModifierName.Testing);
        }

        [TearDown]
        public void TearDown()
        {
            mpu?.Dispose();
            mpu = null;
        }

        [Test]
        public void AddUnit_ShouldCreateValidUnit()
        {
            int id = -1;

            mpu.RequestAddUnit(float2.zero, new float2(999, 999), x => id = x);
            mpu.Tick(0);

            Assert.IsTrue(id >= 0);
        }

        [Test]
        public void AddModifier_ShouldExistInSystem()
        {
            int unitId = -1;
            int handle = -1;

            mpu.RequestAddUnit(float2.zero, new float2(999, 999), x => unitId = x);
            mpu.Tick(0);

            mpu.RequestAddAction(unitId, new DummyMovementAction(), h => handle = h);
            mpu.Tick(0);

            Assert.IsTrue(mpu.HasModifier(handle));
        }

        [Test]
        public void RemoveModifier_ShouldRemoveCorrectly()
        {
            int unitId = -1;
            int handle = -1;

            mpu.RequestAddUnit(float2.zero, new float2(999, 999), x => unitId = x);
            mpu.Tick(0);

            mpu.RequestAddAction(unitId, new DummyMovementAction(), h => handle = h);
            mpu.Tick(0);

            mpu.RequestRemoveAction(handle);
            mpu.Tick(0);

            Assert.IsFalse(mpu.HasModifier(handle));
        }

        [Test]
        public void RemoveModifier_ShouldKeepOtherModifiersValid()
        {
            int unitId = -1;
            int h1 = -1, h2 = -1, h3 = -1;

            mpu.RequestAddUnit(float2.zero, new float2(999, 999), x => unitId = x);
            mpu.Tick(0);

            mpu.RequestAddAction(unitId, new DummyMovementAction(), h => h1 = h);
            mpu.RequestAddAction(unitId, new DummyMovementAction(), h => h2 = h);
            mpu.RequestAddAction(unitId, new DummyMovementAction(), h => h3 = h);
            mpu.Tick(0);

            mpu.RequestRemoveAction(h2);
            mpu.Tick(0);

            Assert.IsTrue(mpu.HasModifier(h1));
            Assert.IsTrue(mpu.HasModifier(h3));
        }

        [Test]
        public void RemoveUnit_ShouldRemoveAllModifiers()
        {
            int unitId = -1;
            int h1 = -1, h2 = -1;

            mpu.RequestAddUnit(float2.zero, new float2(999, 999), x => unitId = x);
            mpu.Tick(0);

            mpu.RequestAddAction(unitId, new DummyMovementAction(), h => h1 = h);
            mpu.RequestAddAction(unitId, new DummyMovementAction(), h => h2 = h);
            mpu.Tick(0);

            mpu.RequestRemoveUnit(unitId);
            mpu.Tick(0);

            Assert.IsFalse(mpu.HasModifier(h1));
            Assert.IsFalse(mpu.HasModifier(h2));
        }

        [Test]
        public void Request_ShouldNotApplyImmediately()
        {
            int unitId = -1;

            mpu.RequestAddUnit(float2.zero, new float2(999, 999), x => unitId = x);

            Assert.AreEqual(-1, unitId);

            mpu.Tick(0);

            Assert.IsTrue(unitId >= 0);
        }
    }

    class DummyMovementAction : IMovementAction
    {
        public int Priority { get; }
        public ModifierName Name => ModifierName.Testing;

        public float2 EvaluatePosition(float deltaTime)
        {
            return float2.zero;
        }

        public bool IsFinished => false;
        public ModifierCompleteReason Reason => ModifierCompleteReason.EndLifeCycle;

        public bool OverrideOthers { get; }

        public DummyMovementAction(int priority = 0, bool overrideOthers = false)
        {
            Priority = priority;
            OverrideOthers = overrideOthers;
        }

        public float2 EvaluateVelocity(float deltaTime)
        {
            return float2.zero;
        }

        public void Start(float2 pos)
        {
        }

        public void Process(float2 position, float deltaTime)
        {

        }

        public void Stop()
        {
        }

        public void Interrupt()
        {
        }
    }
}
