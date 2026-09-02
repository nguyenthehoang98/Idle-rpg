namespace _TDS.Gameplay.Entity
{
    public struct StatData
    {
        public readonly int Exp;
        public readonly int Attack;

        public StatData(int exp, int attack)
        {
            Exp = exp;
            Attack = attack;
        }
    }
}