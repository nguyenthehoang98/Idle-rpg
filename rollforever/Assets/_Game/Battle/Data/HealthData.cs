namespace _Game.Battle.Data
{
    public struct HealthData
    {
        public readonly int maxHealth;
        public int health;

        public HealthData(int maxHealth)
        {
            this.maxHealth = maxHealth;
            this.health = maxHealth;
        }
    }
}