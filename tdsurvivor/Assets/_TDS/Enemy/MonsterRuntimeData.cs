namespace _TDS.Enemy
{
    public struct MonsterRuntimeData
    {
        public readonly float HealthScale;
        public readonly float AttackScale;
        public readonly float ExpScale;
        public readonly float SizeScale;

        public MonsterRuntimeData(float healthScale, float attackScale, float expScale, float sizeScale)
        {
            HealthScale = healthScale;
            AttackScale = attackScale;
            ExpScale = expScale;
            SizeScale = sizeScale;
        }
    }
}