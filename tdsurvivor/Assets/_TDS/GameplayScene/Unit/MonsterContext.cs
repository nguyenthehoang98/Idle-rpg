namespace _TDS.GameplayScene.Unit
{
    public struct MonsterContext
    {
        public readonly float HealthScale;
        public readonly float AttackScale;
        public readonly float ExpScale;
        public readonly float SizeScale;

        public MonsterContext(float healthScale, float attackScale, float expScale, float sizeScale)
        {
            HealthScale = healthScale;
            AttackScale = attackScale;
            ExpScale = expScale;
            SizeScale = sizeScale;
        }
    }
}