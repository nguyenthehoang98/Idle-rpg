namespace _TDS.GameplayScene.SkillSystem
{
    public struct DamageContext
    {
        public int HitCount;
        public float HitInterval;
        public float DamageInterval;

        public DamageContext(int hitCount, float hitInterval, float damageInterval)
        {
            HitCount = hitCount;
            HitInterval = hitInterval;
            DamageInterval = damageInterval;
        }
    }
}