using NUnit.Framework;
using _TDS.Gameplay;

namespace _TDS.Tests.Editor
{
    public class RunSelectionTests
    {
        [SetUp]
        public void SetUp()
        {
            RunSelection.Reset();
        }

        [Test]
        public void NewRunUsesLevelOne()
        {
            Assert.That(RunSelection.SelectedLevel, Is.EqualTo(1));
        }

        [Test]
        public void SelectedLevelSurvivesUntilReset()
        {
            RunSelection.SelectLevel(5);

            Assert.That(RunSelection.SelectedLevel, Is.EqualTo(5));

            RunSelection.Reset();

            Assert.That(RunSelection.SelectedLevel, Is.EqualTo(1));
        }

        [Test]
        public void LevelMustBePositive()
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(() => RunSelection.SelectLevel(0));
        }
    }
}
