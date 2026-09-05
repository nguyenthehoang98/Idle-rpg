namespace _TDS.Gameplay
{
    public sealed class BattleRunRewards
    {
        public int Experience { get; private set; }
        public int Gold { get; private set; }
        public int DefeatedMonsters { get; private set; }

        public void Add(int experience, int gold)
        {
            Experience += System.Math.Max(0, experience);
            Gold += System.Math.Max(0, gold);
            DefeatedMonsters++;
        }
    }
}
