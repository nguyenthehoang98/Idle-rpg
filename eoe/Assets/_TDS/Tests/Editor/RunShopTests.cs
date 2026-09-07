using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using _TDS.Gameplay;

namespace _TDS.Tests.Editor
{
    public sealed class RunShopTests
    {
        [Test]
        public void ShopGeneratesExpectedOfferShapeAndBuildCoverage()
        {
            RunShop shop = RunShop.Create(Catalog(), new[] { "Energy", "Defense" }, seed: 7);

            Assert.That(shop.Offers.Count, Is.EqualTo(5));
            Assert.That(shop.Offers.Count(offer => offer.Kind == RunOfferKind.Hero), Is.EqualTo(1));
            Assert.That(shop.Offers.Count(offer => offer.Kind == RunOfferKind.Item), Is.EqualTo(3));
            Assert.That(shop.Offers.Count(offer => offer.Kind == RunOfferKind.Upgrade), Is.EqualTo(1));
            Assert.That(shop.Offers.Count(IsBuildRelevant), Is.GreaterThanOrEqualTo(2));
            Assert.That(shop.Offers.Count(offer => !IsBuildRelevant(offer)), Is.GreaterThanOrEqualTo(1));
        }

        [Test]
        public void SameSeedProducesSameOffers()
        {
            RunShop first = RunShop.Create(Catalog(), new[] { "Energy" }, seed: 11);
            RunShop second = RunShop.Create(Catalog(), new[] { "Energy" }, seed: 11);

            Assert.That(first.Offers.Select(offer => offer.Id), Is.EqualTo(second.Offers.Select(offer => offer.Id)));
        }

        [Test]
        public void RefreshIsAvailableOnlyOnceByDefault()
        {
            RunShop shop = RunShop.Create(Catalog(), new[] { "Energy" }, seed: 3);

            Assert.That(shop.RefreshesRemaining, Is.EqualTo(1));
            Assert.That(shop.TryRefresh(), Is.True);
            Assert.That(shop.RefreshesRemaining, Is.EqualTo(0));
            Assert.That(shop.TryRefresh(), Is.False);
        }

        private static bool IsBuildRelevant(RunOffer offer)
        {
            foreach (string tag in offer.Tags)
            {
                if (tag == "Energy" || tag == "Defense") return true;
            }

            return false;
        }

        private static IReadOnlyList<RunOffer> Catalog()
        {
            return new[]
            {
                new RunOffer(101, RunOfferKind.Hero, "Energy Hero", 50, "Energy"),
                new RunOffer(102, RunOfferKind.Hero, "Fire Hero", 50, "Fire"),
                new RunOffer(201, RunOfferKind.Item, "Generator", 20, "Energy"),
                new RunOffer(202, RunOfferKind.Item, "Shield", 20, "Defense"),
                new RunOffer(203, RunOfferKind.Item, "Burn", 20, "Fire"),
                new RunOffer(204, RunOfferKind.Item, "Critical", 20, "Critical"),
                new RunOffer(205, RunOfferKind.Item, "Relay", 20, "Energy"),
                new RunOffer(301, RunOfferKind.Upgrade, "Armor", 30, "Defense"),
                new RunOffer(302, RunOfferKind.Upgrade, "Damage", 30, "Critical"),
            };
        }
    }
}
