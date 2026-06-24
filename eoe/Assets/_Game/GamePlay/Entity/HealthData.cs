namespace _BattleSource.Entity
{
    public struct HealthData
    {
        public int MaxHealth;
        public int CurrentHealth;

        public HealthData(int maxHealth)
        {
            CurrentHealth = MaxHealth = maxHealth;
        }
    }
}