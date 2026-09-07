using NUnit.Framework;
using _TDS.Gameplay;

namespace _TDS.Tests.Editor
{
    public sealed class RunOfferTests
    {
        [Test]
        public void OfferExposesShopDataAndStartsAvailable()
        {
            RunOffer offer = new RunOffer(201, RunOfferKind.Item, "Starter Generator", 25, "Energy", "Defense");

            Assert.That(offer.Id, Is.EqualTo(201));
            Assert.That(offer.Kind, Is.EqualTo(RunOfferKind.Item));
            Assert.That(offer.Title, Is.EqualTo("Starter Generator"));
            Assert.That(offer.Price, Is.EqualTo(25));
            Assert.That(offer.Tags, Is.EqualTo(new[] { "Energy", "Defense" }));
            Assert.That(offer.IsAvailable, Is.True);
            Assert.That(offer.IsPurchased, Is.False);
        }

        [Test]
        public void OfferCanBePurchasedOnlyOnce()
        {
            RunOffer offer = new RunOffer(101, RunOfferKind.Hero, "DPS Hero", 50);

            Assert.That(offer.TryPurchase(), Is.True);
            Assert.That(offer.TryPurchase(), Is.False);
            Assert.That(offer.IsAvailable, Is.False);
            Assert.That(offer.IsPurchased, Is.True);
        }

        [Test]
        public void OfferRejectsInvalidIdAndPrice()
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(
                () => new RunOffer(0, RunOfferKind.Upgrade, "Invalid", 1));
            Assert.Throws<System.ArgumentOutOfRangeException>(
                () => new RunOffer(1, RunOfferKind.Upgrade, "Invalid", -1));
        }
    }
}
