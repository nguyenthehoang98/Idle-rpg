namespace _BattleSource.Entity
{
    public struct HealthData
    {
        public int MaxHealth;
        public int CurrentHealth;
        public int FutureHealth;

        public HealthData(int maxHealth)
        {
            FutureHealth = CurrentHealth = MaxHealth = maxHealth;
        }
    }
}