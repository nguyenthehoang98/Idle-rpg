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

    public struct MonsterData
    {
        public int ConfigId;

        public MonsterData(int configId)
        {
            ConfigId = configId;
        }
    }
}