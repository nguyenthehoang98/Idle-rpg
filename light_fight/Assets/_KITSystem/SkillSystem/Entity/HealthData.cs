namespace _KITSystem.SkillSystem.Entity
{
    public struct HealthData
    {
        public int MaxHealth;
        public int CurrentHealth;

        public HealthData(int health)
        {
            MaxHealth = CurrentHealth = health;
        }
    }
}