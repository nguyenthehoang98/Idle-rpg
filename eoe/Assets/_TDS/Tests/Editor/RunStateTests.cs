using NUnit.Framework;
using _TDS.Battle;
using _TDS.Gameplay;

namespace _TDS.Tests.Editor
{
    public sealed class RunStateTests
    {
        [Test]
        public void NewRunStartsEmpty()
        {
            RunState state = new RunState();

            Assert.That(state.Gold, Is.EqualTo(0));
            Assert.That(state.Board.SlotCount, Is.EqualTo(CircuitBoard.DefaultSlotCount));
            Assert.That(state.OwnedHeroIds, Is.Empty);
            Assert.That(state.OwnedItemIds, Is.Empty);
        }

        [Test]
        public void ResetClearsRunState()
        {
            RunState state = new RunState();
            state.AddGold(25);
            state.AddHero(101);
            state.AddItem(201);
            state.Board.SetContent(2, CircuitSlotContent.Hero(101));

            state.Reset();

            Assert.That(state.Gold, Is.EqualTo(0));
            Assert.That(state.OwnedHeroIds, Is.Empty);
            Assert.That(state.OwnedItemIds, Is.Empty);
            Assert.That(state.Board.GetContent(2).Type, Is.EqualTo(CircuitSlotContentType.Empty));
        }

        [Test]
        public void GoldAndOwnershipRejectInvalidValues()
        {
            RunState state = new RunState();

            state.AddGold(-10);

            Assert.That(state.Gold, Is.EqualTo(0));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => state.AddHero(0));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => state.AddItem(0));
        }
    }
}
