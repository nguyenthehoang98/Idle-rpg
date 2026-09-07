using NUnit.Framework;
using _TDS.Battle;
using _TDS.Gameplay;

namespace _TDS.Tests.Editor
{
    public sealed class RunBoardEditingTests
    {
        [Test]
        public void OwnedHeroAndItemCanBePlacedAndSwapped()
        {
            RunState state = PurchasedState();

            Assert.That(state.TryPlaceHero(101, 2), Is.True);
            Assert.That(state.TryPlaceItem(201, 3, CircuitItemType.Generator), Is.True);
            Assert.That(state.Board.GetContent(2).Id, Is.EqualTo(101));
            Assert.That(state.Board.GetItemType(3), Is.EqualTo(CircuitItemType.Generator));

            Assert.That(state.TrySwapSlots(2, 3), Is.True);
            Assert.That(state.Board.GetContent(2).Type, Is.EqualTo(CircuitSlotContentType.Item));
            Assert.That(state.Board.GetContent(3).Type, Is.EqualTo(CircuitSlotContentType.Hero));
        }

        [Test]
        public void UnownedContentAndInvalidSlotsAreRejected()
        {
            RunState state = PurchasedState();

            Assert.That(state.TryPlaceHero(999, 0), Is.False);
            Assert.That(state.TryPlaceHero(101, 8), Is.False);
            Assert.That(state.TryPlaceItem(999, 0, CircuitItemType.Relay), Is.False);
        }

        [Test]
        public void ClearingSlotRemovesBoardContent()
        {
            RunState state = PurchasedState();
            Assert.That(state.TryPlaceHero(101, 2), Is.True);

            Assert.That(state.TryClearSlot(2), Is.True);
            Assert.That(state.Board.GetContent(2).Type, Is.EqualTo(CircuitSlotContentType.Empty));
        }

        private static RunState PurchasedState()
        {
            RunState state = new RunState();
            state.AddGold(100);
            Assert.That(state.TryPurchase(new RunOffer(101, RunOfferKind.Hero, "Hero", 10)), Is.True);
            Assert.That(state.TryPurchase(new RunOffer(201, RunOfferKind.Item, "Item", 10)), Is.True);
            return state;
        }
    }
}
