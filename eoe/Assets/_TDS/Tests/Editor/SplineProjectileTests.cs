using _TDS.Battle;
using NUnit.Framework;
using UnityEngine;

namespace _TDS.Tests.Editor
{
    public sealed class SplineProjectileTests
    {
        [Test]
        public void EvaluatePassesThroughStartAndEnd()
        {
            Vector3 start = new Vector3(-2f, 1f, 0f);
            Vector3 control = new Vector3(0f, 4f, 0f);
            Vector3 end = new Vector3(3f, 2f, 0f);

            Assert.That(SplineProjectile.Evaluate(start, control, end, 0f), Is.EqualTo(start));
            Assert.That(SplineProjectile.Evaluate(start, control, end, 1f), Is.EqualTo(end));
        }

        [Test]
        public void ControlPointUsesMidpointAndConfiguredHeight()
        {
            Vector3 start = new Vector3(-2f, 1f, 0f);
            Vector3 end = new Vector3(4f, 3f, 0f);

            Vector3 control = SplineProjectile.ControlPoint(start, end, 2.5f);

            Assert.That(control, Is.EqualTo(new Vector3(1f, 4.5f, 0f)));
        }
    }
}
