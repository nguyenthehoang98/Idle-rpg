using System;

namespace _KITSystem.Utils
{
    public static class RandomUtils
    {
        static readonly Random RANDOM = new Random();

        public static float Range(float a, float b)
        {
            return Range((int) (a * 1000), (int) (b * 1000)) / 1000f;
        }

        public static int Range(int a, int b)
        {
            return RANDOM.Next(a, b);
        }

        public static float Value => Range(0f, 1f);
    }
}