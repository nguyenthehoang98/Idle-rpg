using System.Collections.Generic;
using NUnit.Framework;
using _TDS.Utils;

namespace _TDS.Tests.Editor
{
    public class GameRngTests
    {
        [Test]
        public void SameSeedProducesSameSequence()
        {
            GameRng.Seed(12345);
            List<float> first = new List<float>();
            for (int i = 0; i < 10; i++) first.Add(GameRng.Value);

            GameRng.Seed(12345);
            for (int i = 0; i < 10; i++)
                Assert.That(GameRng.Value, Is.EqualTo(first[i]));
        }

        [Test]
        public void IntRangeRespectsBounds()
        {
            GameRng.Seed(7);
            for (int i = 0; i < 100; i++)
            {
                int roll = GameRng.Range(1, 8);
                Assert.That(roll, Is.InRange(1, 7));
            }
        }

        [Test]
        public void FloatRangeRespectsBounds()
        {
            GameRng.Seed(7);
            for (int i = 0; i < 100; i++)
            {
                float roll = GameRng.Range(-2.5f, 2.5f);
                Assert.That(roll, Is.InRange(-2.5f, 2.5f));
            }
        }
    }
}
