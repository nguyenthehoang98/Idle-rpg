namespace _Game.Battle.Data
{
    public struct HealthData
    {
        public int health;
        public int maxHealth;

        public HealthData(int health)
        {
            this.maxHealth = this.health = health;
        }
    }
}