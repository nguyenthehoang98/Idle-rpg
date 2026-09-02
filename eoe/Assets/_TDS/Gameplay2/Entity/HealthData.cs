namespace _TDS.Gameplay.Entity
{
    public struct HealthData
    {
        public int MaxHealth;
        public int CurrentHealth;
        public int PredictedHealth;

        public HealthData(int maxHealth)
        {
            CurrentHealth = PredictedHealth = MaxHealth = maxHealth;
        }
    }
}