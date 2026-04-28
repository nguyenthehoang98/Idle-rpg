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
                var run = new RunModifier(Vector3.right, 5f);
                mpu.RequestAddModifier(unit, run);    
            });

            mpu.Tick(1f); // 1 giây

            Vector3 pos = mpu.GetUnitPosition(unitId);

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