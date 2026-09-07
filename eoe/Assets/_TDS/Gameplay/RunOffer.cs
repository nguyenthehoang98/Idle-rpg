using System;
using System.Collections.Generic;

namespace _TDS.Gameplay
{
    public enum RunOfferKind
    {
        Hero,
        Item,
        Upgrade,
    }

    public sealed class RunOffer
    {
        private readonly string[] tags;

        public RunOffer(int id, RunOfferKind kind, string title, int price, params string[] tags)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));
            if (price < 0) throw new ArgumentOutOfRangeException(nameof(price));

            Id = id;
            Kind = kind;
            Title = title ?? string.Empty;
            Price = price;
            this.tags = tags == null ? Array.Empty<string>() : (string[])tags.Clone();
        }

        public int Id { get; }
        public RunOfferKind Kind { get; }
        public string Title { get; }
        public int Price { get; }
        public IReadOnlyList<string> Tags => tags;
        public bool IsPurchased { get; private set; }
        public bool IsAvailable => !IsPurchased;

        public bool TryPurchase()
        {
            if (IsPurchased) return false;
            IsPurchased = true;
            return true;
        }
    }
}
