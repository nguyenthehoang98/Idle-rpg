using System;

namespace _TDS.Gameplay
{
    public static class RunSelection
    {
        public const int DefaultLevel = 1;
        public const int MaxCampaignLevel = 20;

        public static int SelectedLevel { get; private set; } = DefaultLevel;

        public static void SelectLevel(int level)
        {
            if (level <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(level), level, "Level must be positive.");
            }

            SelectedLevel = level;
        }

        public static void Reset()
        {
            SelectedLevel = DefaultLevel;
        }
    }
}
