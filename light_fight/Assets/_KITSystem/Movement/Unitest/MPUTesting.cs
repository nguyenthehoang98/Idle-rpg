using NUnit.Framework;
using UnityEngine;

namespace _KITSystem.Movement.Unitest
{
    public class MPUTesting
    {
        [Test]
        public void AddUnit_ShouldCreateValidUnit()
        {
            var mpu = CreateMPU();

            int id = -1;

            mpu.RequestAddUnit(Vector3.zero, new Vector3(999, 999), x => id = x);
            mpu.Tick(0);

            Assert.IsTrue(id >= 0);
        }

        [Test]
        public void AddModifier_ShouldExistInSystem()
        {
            var mpu = CreateMPU();

            int unitId = -1;
            int handle = -1;

            mpu.RequestAddUnit(Vector3.zero, new Vector3(999, 999), x => unitId = x);
            mpu.Tick(0);

            mpu.RequestAddModifier(unitId, new DummyModifier(), h => handle = h);
            mpu.Tick(0);

            Assert.IsTrue(mpu.HasModifier(handle));
        }

        [Test]
        public void RemoveModifier_ShouldRemoveCorrectly()
        {
            var mpu = CreateMPU();

            int unitId = -1;
            int handle = -1;

            mpu.RequestAddUnit(Vector3.zero, new Vector3(999, 999), x => unitId = x);
            mpu.Tick(0);

            mpu.RequestAddModifier(unitId, new DummyModifier(), h => handle = h);
            mpu.Tick(0);

            mpu.RequestRemoveModifier(handle);
            mpu.Tick(0);

            Assert.IsFalse(mpu.HasModifier(handle));
        }

        [Test]
        public void RemoveModifier_ShouldKeepOtherModifiersValid()
        {
            var mpu = CreateMPU();

            int unitId = -1;
            int h1 = -1, h2 = -1, h3 = -1;

            mpu.RequestAddUnit(Vector3.zero, new Vector3(999, 999), x => unitId = x);
            mpu.Tick(0);

            mpu.RequestAddModifier(unitId, new DummyModifier(), h => h1 = h);
            mpu.RequestAddModifier(unitId, new DummyModifier(), h => h2 = h);
            mpu.RequestAddModifier(unitId, new DummyModifier(), h => h3 = h);
            mpu.Tick(0);

            mpu.RequestRemoveModifier(h2);
            mpu.Tick(0);

            Assert.IsTrue(mpu.HasModifier(h1));
            Assert.IsTrue(mpu.HasModifier(h3));
        }

        [Test]
        public void RemoveUnit_ShouldRemoveAllModifiers()
        {
            var mpu = CreateMPU();

            int unitId = -1;
            int h1 = -1, h2 = -1;

            mpu.RequestAddUnit(Vector3.zero, new Vector3(999, 999), x => unitId = x);
            mpu.Tick(0);

            mpu.RequestAddModifier(unitId, new DummyModifier(), h => h1 = h);
            mpu.RequestAddModifier(unitId, new DummyModifier(), h => h2 = h);
            mpu.Tick(0);

            mpu.RequestRemoveUnit(unitId);
            mpu.Tick(0);

            Assert.IsFalse(mpu.HasModifier(h1));
            Assert.IsFalse(mpu.HasModifier(h2));
        }

        [Test]
        public void Request_ShouldNotApplyImmediately()
        {
            var mpu = CreateMPU();

            int unitId = -1;

            mpu.RequestAddUnit(Vector3.zero, new Vector3(999, 999), x => unitId = x);

            // chưa Tick
            Assert.AreEqual(-1, unitId);

            mpu.Tick(0);

            Assert.IsTrue(unitId >= 0);
        }

        MPU CreateMPU()
        {
            var mpu = new MPU(ModifierName.Testing);
            mpu.Initialize();
            return mpu;
        }
    }

    class DummyModifier : IModifier
    {
        public int Priority { get; }
        public ModifierName Name => ModifierName.Testing;

        public Vector3 EvaluatePosition(float deltaTime)
        {
            return Vector3.zero;
        }

        public bool IsFinished => false;
        public ModifierCompleteReason Reason => ModifierCompleteReason.EndLifeCycle;

        public bool OverrideOthers { get; }

        public DummyModifier(int priority = 0, bool overrideOthers = false)
        {
            Priority = priority;
            OverrideOthers = overrideOthers;
        }

        public Vector3 EvaluateVelocity(float deltaTime)
        {
            return Vector3.zero;
        }

        public void Start(Vector3 pos)
        {
        }

        public void Process(Vector3 position, float deltaTime)
        {
            
        }

        public void Tick(float deltaTime)
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