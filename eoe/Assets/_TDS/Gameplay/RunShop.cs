using System;
using System.Collections.Generic;
using System.Linq;

namespace _TDS.Gameplay
{
    public sealed class RunShop
    {
        private readonly List<RunOffer> catalog;
        private readonly HashSet<string> buildTags;
        private int seed;

        private RunShop(IEnumerable<RunOffer> catalog, IEnumerable<string> buildTags, int seed)
        {
            this.catalog = new List<RunOffer>(catalog ?? throw new ArgumentNullException(nameof(catalog)));
            if (this.catalog.Any(offer => offer == null))
            {
                throw new ArgumentException("Shop catalog cannot contain null offers.", nameof(catalog));
            }

            this.buildTags = new HashSet<string>(
                buildTags ?? Array.Empty<string>(),
                StringComparer.OrdinalIgnoreCase);
            this.seed = seed;
            Offers = GenerateOffers(seed);
        }

        public IReadOnlyList<RunOffer> Offers { get; private set; }
        public int RefreshesRemaining { get; private set; } = 1;

        public static RunShop Create(
            IEnumerable<RunOffer> catalog,
            IEnumerable<string> buildTags,
            int seed)
        {
            return new RunShop(catalog, buildTags, seed);
        }

        public bool TryRefresh()
        {
            if (RefreshesRemaining <= 0) return false;

            int nextSeed = seed + 1;
            List<RunOffer> refreshed = GenerateOffers(nextSeed);
            seed = nextSeed;
            Offers = refreshed;
            RefreshesRemaining--;
            return true;
        }

        private List<RunOffer> GenerateOffers(int generationSeed)
        {
            List<RunOffer> heroes = Available(RunOfferKind.Hero);
            List<RunOffer> items = Available(RunOfferKind.Item);
            List<RunOffer> upgrades = Available(RunOfferKind.Upgrade);
            if (heroes.Count < 1 || items.Count < 3 || upgrades.Count < 1)
            {
                throw new InvalidOperationException("Shop catalog does not contain enough available offers.");
            }

            for (int attempt = 0; attempt < 100; attempt++)
            {
                Random random = new Random(generationSeed + attempt);
                List<RunOffer> result = new List<RunOffer>();
                result.AddRange(Pick(heroes, 1, random));
                result.AddRange(Pick(items, 3, random));
                result.AddRange(Pick(upgrades, 1, random));

                int relevant = result.Count(IsBuildRelevant);
                if (relevant >= 2 && relevant < result.Count)
                {
                    return result;
                }
            }

            throw new InvalidOperationException(
                "Shop catalog cannot satisfy build coverage constraints.");
        }

        private List<RunOffer> Available(RunOfferKind kind)
        {
            return catalog
                .Where(offer => offer.Kind == kind && offer.IsAvailable)
                .ToList();
        }

        private List<RunOffer> Pick(List<RunOffer> source, int count, Random random)
        {
            List<RunOffer> pool = new List<RunOffer>(source);
            List<RunOffer> result = new List<RunOffer>(count);
            for (int i = 0; i < count; i++)
            {
                int index = random.Next(pool.Count);
                result.Add(pool[index]);
                pool.RemoveAt(index);
            }

            return result;
        }

        private bool IsBuildRelevant(RunOffer offer)
        {
            return offer.Tags.Any(tag => tag != null && buildTags.Contains(tag));
        }
    }
}
