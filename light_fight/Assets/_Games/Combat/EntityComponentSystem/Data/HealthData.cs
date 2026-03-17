using Unity.Entities;

namespace _Games.Combat.EntityComponentSystem.Data
{
    public struct HealthData : IComponentData
    {
        public int MaxHealth;
        public int Health;
    }
}