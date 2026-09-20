using System;

namespace _TDS.Utils
{
    /// <summary>
    /// Single seeded RNG for all gameplay rolls. Seed once per run for deterministic replays.
    /// </summary>
    public static class GameRng
    {
        private static Random rng = new Random();

        public static void Seed(int seed)
        {
            rng = new Random(seed);
        }

        public static int Range(int minInclusive, int maxExclusive)
        {
            return rng.Next(minInclusive, maxExclusive);
        }

        public static float Range(float minInclusive, float maxInclusive)
        {
            double t = rng.NextDouble();
            return (float)(minInclusive + t * (maxInclusive - minInclusive));
        }

        public static float Value => (float)rng.NextDouble();
    }
}
