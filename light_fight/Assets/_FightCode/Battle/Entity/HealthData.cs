namespace _KITSystem.Entity
{
    public struct HealthData
    {
        public int MaxHealth;
        public int CurrentHealth;
        public int TargetHealth;

        public HealthData(int health)
        {
            MaxHealth = CurrentHealth = TargetHealth = health;
        }
    }
}