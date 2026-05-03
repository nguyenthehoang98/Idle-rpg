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
            mpu.RequestAddUnit(Vector3.zero, new Vector3(999, 999), unit =>
            {
                unitId = unit;
                mpu.RequestAddModifier(unit, new RunModifier(Vector3.right, 5f));
            });

            mpu.Tick(1f); // 1 giây

            Vector3 pos = mpu.GetUnitPosition(unitId);

            Assert.AreEqual(new Vector3(5f, 0f, 0f), pos);
        }

        [Test]
        public void RunModifier_ShouldSumVelocity()
        {
            var mpu = CreateMPU();

            int unitId = -1;
            mpu.RequestAddUnit(Vector3.zero, new Vector3(999, 999), unit =>
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
        public void RunModifier_ShouldStopMovement()
        {
            var mpu = CreateMPU();

            int unitId = -1;
            int handle = -1;
            mpu.RequestAddUnit(Vector3.zero, new Vector3(999, 999), unit =>
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
        public void RunModifier_ShouldKeepOtherModifiersValid()
        {
            var mpu = CreateMPU();

            int unitId = -1, h1 = -1, h2 = -1;
            mpu.RequestAddUnit(Vector3.zero, new Vector3(999, 999), unit =>
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
        public void RunModifier_SameFrame_ShouldNotCrash()
        {
            var mpu = CreateMPU();

            int unitId = -1;
            int handle = -1;

            mpu.RequestAddUnit(Vector3.zero, new Vector3(999, 999), unit =>
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

        [Test]
        public void TeleportModifier_ShouldMoveUnit()
        {
            var mpu = CreateMPU();

            int unitId = -1;
            mpu.RequestAddUnit(Vector3.zero, new Vector3(999, 999), unit =>
            {
                unitId = unit;
                mpu.RequestAddModifier(unit, new TeleportModifier(new Vector3(33, 0)));
            });

            mpu.Tick(Random.Range(0.1f, 10f)); // 1 giây

            Vector3 pos = mpu.GetUnitPosition(unitId);

            Assert.AreEqual(new Vector3(33, 0), pos);
        }
        
        [Test]
        public void KnockBackModifier_Move_Should_Be_Ignored_When_Knockback_Active()
        {
            var mpu = CreateMPU();

            int unitId = -1;
            mpu.RequestAddUnit(Vector3.zero, new Vector3(999, 999), unit =>
            {
                unitId = unit;
                mpu.RequestAddModifier(unit, new RunModifier(new Vector3(1, 0, 0), speed: 5f));
                mpu.RequestAddModifier(unit, new KnockBackModifier(
                    direction: new Vector3(-1, 0, 0), 2f, 1f, null)
                );
            });

            mpu.Tick(1f);
           
            Vector3 pos = mpu.GetUnitPosition(unitId);

            // phải bị đẩy ngược, không đi theo move
            Assert.Less(pos.x, 0f);
        }
        
        [Test]
        public void KnockBackModifier_Distance()
        {
            var mpu = CreateMPU();

            int unitId = -1;
            mpu.RequestAddUnit(Vector3.zero, new Vector3(999, 999), unit =>
            {
                unitId = unit;
                mpu.RequestAddModifier(unit, new KnockBackModifier(
                    direction: new Vector3(-1, 0, 0), 2f, 3f, null)
                );
            });

            for (int i = 0; i < 10; i++)
            {
                mpu.Tick(0.2f);
            }
           
            Vector3 pos = mpu.GetUnitPosition(unitId);

            // phải bị đẩy ngược, không đi theo move
            Assert.AreEqual(pos.x, -3f);
        }
        
        Mpu CreateMPU()
        {
            var mpu = new Mpu(ModifierName.Default, ModifierName.Teleport, ModifierName.KnockBack);
            mpu.Initialize();
            return mpu;
        }
    }
}