namespace _TDS.GameplayScene.Unit
{
    public struct HealthComponent
    {
        public int MaxHealth;
        public int CurrentHealth;
        public int PredictedHealth;

        public HealthComponent(int maxHealth)
        {
            CurrentHealth = PredictedHealth = MaxHealth = maxHealth;
        }
    }
}