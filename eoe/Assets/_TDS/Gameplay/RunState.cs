using System;
using System.Collections.Generic;
using _TDS.Battle;

namespace _TDS.Gameplay
{
    public sealed class RunState
    {
        private readonly HashSet<int> ownedHeroIds = new HashSet<int>();
        private readonly HashSet<int> ownedItemIds = new HashSet<int>();

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

        private static void ValidateId(int id, string name)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(name, id, "Content id must be positive.");
        }
    }
}
