using System.Linq;
using NUnit.Framework;
using _TDS.Gameplay;

namespace _TDS.Tests.Editor
{
    public sealed class RunTransactionsTests
    {
        [Test]
        public void PurchaseConsumesGoldAndTracksHero()
        {
            RunState state = new RunState();
            RunOffer offer = new RunOffer(101, RunOfferKind.Hero, "DPS Hero", 50);
            state.AddGold(50);

            Assert.That(state.TryPurchase(offer), Is.True);
            Assert.That(state.Gold, Is.EqualTo(0));
            Assert.That(state.OwnedHeroIds, Does.Contain(101));
            Assert.That(state.TryPurchase(offer), Is.False);
        }

        [Test]
        public void PurchaseFailsWithoutEnoughGoldAndLeavesOfferAvailable()
        {
            RunState state = new RunState();
            RunOffer offer = new RunOffer(201, RunOfferKind.Item, "Generator", 20);
            state.AddGold(10);

            Assert.That(state.TryPurchase(offer), Is.False);
            Assert.That(state.Gold, Is.EqualTo(10));
            Assert.That(offer.IsAvailable, Is.True);
            Assert.That(state.OwnedItemIds.Contains(201), Is.False);
        }

        [Test]
        public void SellingPurchasedItemRefundsItsOriginalPrice()
        {
            RunState state = new RunState();
            RunOffer offer = new RunOffer(201, RunOfferKind.Item, "Generator", 35);
            state.AddGold(35);
            Assert.That(state.TryPurchase(offer), Is.True);
            Assert.That(state.TryPlaceItem(201, 2, _TDS.Battle.CircuitItemType.Generator), Is.True);

            Assert.That(state.TrySellItem(201), Is.True);
            Assert.That(state.Gold, Is.EqualTo(35));
            Assert.That(state.Board.GetContent(2).Type, Is.EqualTo(_TDS.Battle.CircuitSlotContentType.Empty));
            Assert.That(state.OwnedItemIds.Contains(201), Is.False);
            Assert.That(state.TrySellItem(201), Is.False);
        }
    }
}
