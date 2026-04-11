namespace _Games.Combat
{
    public static class BattleTime
    {
        public static bool IsRunning { get; set; } = false;
        public static float ScaleTime { get; set; } = 1f;
        public static float LastScaleTime { get; set; } = 0f;
        public static float DeltaTime { get; set; }
        public static float Time { get; set; }
    }
}