using NUnit.Framework;
using UnityEngine;

namespace _KITSystem.Movement.Testing
{
    public class MPUModifierTesting
    {
        [Test]
        public void RunModifier_ShouldMoveUnit()
        {
            var mpu = CreateMPU();

            int unitId = -1;
            mpu.RequestAddUnit(Vector3.zero, unit =>
            {
                unitId = unit;
                mpu.RequestAddModifier(unit,  new RunModifier(Vector3.right, 5f));    
            });

            mpu.Tick(1f); // 1 giây

            Vector3 pos = mpu.GetUnitPosition(unitId);

            Assert.AreEqual(new Vector3(5f, 0f, 0f), pos);
        }
        
        [Test]
        public void MultipleRunModifier_ShouldSumVelocity()
        {
            var mpu = new MPU();
            mpu.Initialize();
            
            int unitId = -1;
            mpu.RequestAddUnit(Vector3.zero, unit =>
            {
                unitId = unit;
                mpu.RequestAddModifier(unit, new RunModifier(Vector3.right, 5f));
                mpu.RequestAddModifier(unit, new RunModifier(Vector3.forward, 5f));
            });

            mpu.Tick(2f);

            Vector3 pos = mpu.GetUnitPosition(unitId);

            Assert.AreEqual(new Vector3(10f, 0f, 10f), pos);
        }
        
        [Test]
        public void RemoveModifier_ShouldStopMovement()
        {
            var mpu = new MPU();
            mpu.Initialize();

            int unitId = -1;
            int handle = -1;
            mpu.RequestAddUnit(Vector3.zero, unit =>
            {
                unitId = unit;
                mpu.RequestAddModifier(unit, new RunModifier(Vector3.right, 5f), i => handle = i);
            });

            mpu.Tick(1f);

            mpu.RequestRemoveModifier(handle);

            mpu.Tick(1f);

            Vector3 pos = mpu.GetUnitPosition(unitId);

            // chỉ move 1 lần
            Assert.AreEqual(new Vector3(5f, 0f, 0f), pos);
        }
        
        [Test]
        public void RemoveModifier_ShouldKeepOtherModifiersValid()
        {
            var mpu = new MPU();
            mpu.Initialize();
            
            int unitId = -1, h1 = -1, h2 = -1;
            mpu.RequestAddUnit(Vector3.zero, unit =>
            {
                unitId = unit;
                mpu.RequestAddModifier(unit, new RunModifier(Vector3.right, 5f), i => h1 = i);
                mpu.RequestAddModifier(unit, new RunModifier(Vector3.forward, 5f), i => h2 = i);
            });

            mpu.Tick(1f);

            mpu.RequestRemoveModifier(h1);

            mpu.Tick(1f);

            Vector3 pos = mpu.GetUnitPosition(unitId);

            // frame 1: (5,0,5)
            // frame 2: chỉ còn forward → (0,0,5)
            Assert.AreEqual(new Vector3(5f, 0f, 10f), pos);
        }
        
        [Test]
        public void RemoveModifier_SameFrame_ShouldNotCrash()
        {
            var mpu = new MPU();
            mpu.Initialize();

            int unitId = -1;
            int handle = -1;

            mpu.RequestAddUnit(Vector3.zero, unit =>
            {
                unitId = unit;
                mpu.RequestAddModifier(unit, new RunModifier(Vector3.right, 5f), i => handle = i);
            });

            mpu.Tick(1f); // chạy → move

            mpu.RequestRemoveModifier(handle);

            mpu.Tick(1f); // apply remove

            Vector3 pos = mpu.GetUnitPosition(unitId);

            // chỉ move 1 lần
            Assert.AreEqual(new Vector3(5f, 0f, 0f), pos);
        }

        MPU CreateMPU()
        {
            var mpu = new MPU(ModifierName.Testing);
            mpu.Initialize();
            return mpu;
        }
    }
}