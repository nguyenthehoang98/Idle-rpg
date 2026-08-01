namespace _TDS.GameplayScene.Statistics
{
    /// <summary>
    /// Hàm này để sử dụng trong code, phải dối chiếu với file config xem đúng Id ko tránh sai sót 
    /// </summary>
    public static class StatsIdConstant
    {
        // Vital
        public const int HP = 1001;
        public const int HP_MAX = 1002;
        public const int MP = 1003;
        public const int MP_MAX = 1004;

        // Combat
        public const int ATK = 2001;
        public const int DEF = 2002;
        public const int MAGIC_ATK = 2003;
        public const int MAGIC_DEF = 2004;
        public const int CRIT_RATE = 2005;
        public const int CRIT_DAMAGE = 2006;

        // Movement
        public const int MOVE_SPEED = 3001;
        public const int ATTACK_SPEED = 3002;

        // Economy
        public const int GOLD_GAIN = 4001;
        public const int EXP_GAIN = 4002;
    }
}