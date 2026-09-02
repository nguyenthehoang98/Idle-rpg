namespace _TDS.Battle
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