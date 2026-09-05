using System;
using System.Collections.Generic;

namespace _TDS.Gameplay
{
    public sealed class BattleRunRewards
    {
        private readonly List<int> selectedUpgradeIds = new List<int>();

        public int Experience { get; private set; }
        public int Gold { get; private set; }
        public int DefeatedMonsters { get; private set; }
        public IReadOnlyList<int> SelectedUpgradeIds => selectedUpgradeIds;

        public void Add(int experience, int gold)
        {
            Experience += System.Math.Max(0, experience);
            Gold += System.Math.Max(0, gold);
            DefeatedMonsters++;
        }

        public bool TrySpendGold(int amount)
        {
            if (amount < 0 || Gold < amount) return false;
            Gold -= amount;
            return true;
        }

        public void RefundGold(int amount)
        {
            Gold += Math.Max(0, amount);
        }

        public void RecordUpgrade(int cardId)
        {
            selectedUpgradeIds.Add(cardId);
        }
    }
}
