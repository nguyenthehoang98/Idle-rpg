namespace _Game.GamePlay.Data
{
    public struct SkillRuntimeData
    {
        public float Attack;
        public float CritChance;
        public float CritDamage;
        public int ParallelCount;
        public float ParallelDamagePercent;
        public int SpreadCount;
        public float SpreadDamagePercent;
        public int PiercingCount;
        public float ExplosiveRadius;
        public float ExplosiveDamagePercent;
        public int BounceCount; // chưa có logic
        public float BounceDamagePercent; // chưa có logic
        public float KillInstantBelowHealthPercent;
    }
}