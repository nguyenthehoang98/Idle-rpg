using _KIT.Event;

namespace _Game.Battle.Ecs.Events
{
    public readonly struct DamagePlayerEvent : IEvent
    {
        public readonly int MaxHealth;
        public readonly int CurrentHealth;
        public readonly int Damage;

        public DamagePlayerEvent(int damage, int currentHealth, int maxHealth)
        {
            Damage = damage;
            CurrentHealth = currentHealth;
            MaxHealth = maxHealth;
        }
    }
}