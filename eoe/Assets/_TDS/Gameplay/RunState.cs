using System;
using System.Collections.Generic;
using _TDS.Battle;

namespace _TDS.Gameplay
{
    public sealed class RunState
    {
        private readonly HashSet<int> ownedHeroIds = new HashSet<int>();
        private readonly HashSet<int> ownedItemIds = new HashSet<int>();
        private readonly Dictionary<int, int> purchasedHeroPrices = new Dictionary<int, int>();
        private readonly Dictionary<int, int> purchasedItemPrices = new Dictionary<int, int>();

        public RunState()
        {
            Board = new CircuitBoard();
        }

        public int Gold { get; private set; }
        public CircuitBoard Board { get; }
        public IReadOnlyCollection<int> OwnedHeroIds => ownedHeroIds;
        public IReadOnlyCollection<int> OwnedItemIds => ownedItemIds;

        public void Reset()
        {
            Gold = 0;
            ownedHeroIds.Clear();
            ownedItemIds.Clear();
            purchasedHeroPrices.Clear();
            purchasedItemPrices.Clear();
            Board.Reset();
        }

        public void AddGold(int amount)
        {
            if (amount > 0) Gold += amount;
        }

        public bool AddHero(int heroId)
        {
            ValidateId(heroId, nameof(heroId));
            return ownedHeroIds.Add(heroId);
        }

        public bool AddItem(int itemId)
        {
            ValidateId(itemId, nameof(itemId));
            return ownedItemIds.Add(itemId);
        }

        public bool TryPurchase(RunOffer offer)
        {
            if (offer == null || !offer.IsAvailable || offer.Kind == RunOfferKind.Upgrade || Gold < offer.Price)
            {
                return false;
            }

            HashSet<int> ownedIds = offer.Kind == RunOfferKind.Hero ? ownedHeroIds : ownedItemIds;
            Dictionary<int, int> prices = offer.Kind == RunOfferKind.Hero
                ? purchasedHeroPrices
                : purchasedItemPrices;
            if (ownedIds.Contains(offer.Id) || !offer.TryPurchase()) return false;

            Gold -= offer.Price;
            ownedIds.Add(offer.Id);
            prices[offer.Id] = offer.Price;
            return true;
        }

        public bool TrySellHero(int heroId)
        {
            return TrySell(heroId, ownedHeroIds, purchasedHeroPrices);
        }

        public bool TrySellItem(int itemId)
        {
            return TrySell(itemId, ownedItemIds, purchasedItemPrices);
        }

        public bool TryPlaceHero(int heroId, int slotIndex)
        {
            if (!IsValidSlot(slotIndex) || !ownedHeroIds.Contains(heroId)) return false;
            Board.SetContent(slotIndex, CircuitSlotContent.Hero(heroId));
            return true;
        }

        public bool TryPlaceItem(int itemId, int slotIndex, CircuitItemType itemType, int power = 1)
        {
            if (!IsValidSlot(slotIndex) || !ownedItemIds.Contains(itemId) || itemType == CircuitItemType.None || power <= 0)
            {
                return false;
            }

            Board.SetItem(slotIndex, itemId, itemType, power);
            return true;
        }

        public bool TrySwapSlots(int firstIndex, int secondIndex)
        {
            if (!IsValidSlot(firstIndex) || !IsValidSlot(secondIndex)) return false;
            Board.SwapSlots(firstIndex, secondIndex);
            return true;
        }

        public bool TryClearSlot(int slotIndex)
        {
            if (!IsValidSlot(slotIndex)) return false;
            Board.ClearSlot(slotIndex);
            return true;
        }

        private bool IsValidSlot(int index)
        {
            return index >= 0 && index < Board.SlotCount;
        }

        private bool TrySell(int id, HashSet<int> ownedIds, Dictionary<int, int> prices)
        {
            if (!prices.TryGetValue(id, out int price)) return false;

            prices.Remove(id);
            ownedIds.Remove(id);
            Gold += price;
            return true;
        }

        private static void ValidateId(int id, string name)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(name, id, "Content id must be positive.");
        }
    }
}
