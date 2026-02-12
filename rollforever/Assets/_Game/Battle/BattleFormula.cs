namespace _Game.Battle
{
    public static class BattleFormula
    {
        public static int PowerMonster(int id, int level)
        {
            return id * level;
        }
    }
}