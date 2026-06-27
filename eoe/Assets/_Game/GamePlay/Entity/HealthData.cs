namespace _Game.GamePlay.Entity
{
    public struct HealthData
    {
        public int MaxHealth;
        public int CurrentHealth;
        public int FutureHealth;

        public HealthData(int maxHealth)
        {
            CurrentHealth = FutureHealth = MaxHealth = maxHealth;
        }
    }
}