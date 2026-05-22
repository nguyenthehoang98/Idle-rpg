using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _KITSystem.Movement.Unitest
{
    public class MPUModifierTesting
    {
        private MPU mpu;

        [SetUp]
        public void Setup()
        {
            mpu = new MPU(ModifierName.Default, ModifierName.Teleport, ModifierName.KnockBack);
            mpu.Initialize();
        }

        [TearDown]
        public void TearDown()
        {
            mpu?.Dispose();
            mpu = null;
        }

        [Test]
        public void RunModifier_ShouldMoveUnit()
        {
            int unitId = -1;
            mpu.RequestAddUnit(float2.zero, new float2(999, 999), unit =>
            {
                unitId = unit;
                mpu.RequestAddAction(unit, new RunMovementAction(Vector2.right, 5f));
            });

            mpu.Tick(1f);

            float2 pos = mpu.GetUnitPosition(unitId);

            Assert.AreEqual(new float2(5f, 0f), pos);
        }

        [Test]
        public void RunModifier_ShouldSumVelocity()
        {
            int unitId = -1;
            mpu.RequestAddUnit(float2.zero, new float2(999, 999), unit =>
            {
                unitId = unit;
                mpu.RequestAddAction(unit, new RunMovementAction(Vector2.right, 5f));
                mpu.RequestAddAction(unit, new RunMovementAction(Vector2.up, 5f));
            });

            mpu.Tick(2f);

            float2 pos = mpu.GetUnitPosition(unitId);

            Assert.AreEqual(new float2(10f, 10f), pos);
        }

        [Test]
        public void RunModifier_ShouldStopMovement()
        {
            int unitId = -1;
            int handle = -1;
            mpu.RequestAddUnit(float2.zero, new float2(999, 999), unit =>
            {
                unitId = unit;
                mpu.RequestAddAction(unit, new RunMovementAction(Vector2.right, 5f), i => handle = i);
            });

            mpu.Tick(1f);

            mpu.RequestRemoveAction(handle);

            mpu.Tick(1f);

            float2 pos = mpu.GetUnitPosition(unitId);

            Assert.AreEqual(new float2(5f, 0f), pos);
        }

        [Test]
        public void RunModifier_ShouldKeepOtherModifiersValid()
        {
            int unitId = -1, h1 = -1, h2 = -1;
            mpu.RequestAddUnit(float2.zero, new float2(999, 999), unit =>
            {
                unitId = unit;
                mpu.RequestAddAction(unit, new RunMovementAction(Vector2.right, 5f), i => h1 = i);
                mpu.RequestAddAction(unit, new RunMovementAction(Vector2.up, 5f), i => h2 = i);
            });

            mpu.Tick(1f);

            mpu.RequestRemoveAction(h1);

            mpu.Tick(1f);

            float2 pos = mpu.GetUnitPosition(unitId);

            Assert.AreEqual(new float2(5f, 10f), pos);
        }

        [Test]
        public void RunModifier_SameFrame_ShouldNotCrash()
        {
            int unitId = -1;
            int handle = -1;

            mpu.RequestAddUnit(float2.zero, new float2(999, 999), unit =>
            {
                unitId = unit;
                mpu.RequestAddAction(unit, new RunMovementAction(Vector2.right, 5f), i => handle = i);
            });

            mpu.Tick(1f);

            mpu.RequestRemoveAction(handle);

            mpu.Tick(1f);

            float2 pos = mpu.GetUnitPosition(unitId);

            Assert.AreEqual(new float2(5f, 0f), pos);
        }

        [Test]
        public void TeleportModifier_ShouldMoveUnit()
        {
            int unitId = -1;
            mpu.RequestAddUnit(float2.zero, new float2(999, 999), unit =>
            {
                unitId = unit;
                mpu.RequestAddAction(unit, new TeleportMovementAction(new Vector3(33, 0, 0)));
            });

            mpu.Tick(Random.Range(0.1f, 10f));

            float2 pos = mpu.GetUnitPosition(unitId);

            Assert.AreEqual(new float2(33, 0), pos);
        }

        [Test]
        public void KnockBackModifier_Move_Should_Be_Ignored_When_Knockback_Active()
        {
            int unitId = -1;
            mpu.RequestAddUnit(float2.zero, new float2(999, 999), unit =>
            {
                unitId = unit;
                mpu.RequestAddAction(unit, new RunMovementAction(new Vector2(1, 0), speed: 5f));
                mpu.RequestAddAction(unit, new KnockBackMovementAction(
                    direction: new Vector2(-1, 0), 2f, 1f, null)
                );
            });

            mpu.Tick(1f);

            float2 pos = mpu.GetUnitPosition(unitId);

            Assert.Less(pos.x, 0f);
        }

        [Test]
        public void KnockBackModifier_Distance()
        {
            int unitId = -1;
            mpu.RequestAddUnit(float2.zero, new float2(999, 999), unit =>
            {
                unitId = unit;
                mpu.RequestAddAction(unit, new KnockBackMovementAction(
                    direction: new Vector2(-1, 0), 2f, 3f, null)
                );
            });

            for (int i = 0; i < 10; i++)
            {
                mpu.Tick(0.2f);
            }

            float2 pos = mpu.GetUnitPosition(unitId);

            Assert.AreEqual(-3f, pos.x, 0.001f);
        }
    }
}
