namespace _Games.Misc.Model
{
    public enum Rarity
    {
        Common = 0,
        Uncommon = 1,
        Rare = 2,
        Epic = 3,
        Legendary = 4,
    }

    public static class RarityMethod
    {
        public static Rarity MinRarity => Rarity.Common;
        public static Rarity MaxRarity => Rarity.Legendary;

        public static Rarity IncreaseRarity(Rarity rarity)
        {
            if (rarity < MaxRarity)
            {
                int index = (int)rarity;
                return (Rarity)(index + 1);
            }

            return rarity;
        }

        public static Rarity ParseRarity(int level)
        {
            return (Rarity)level;
        }

        public static int ParseLevel(Rarity rarity)
        {
            return (int)rarity + 1;
        }
    }
}