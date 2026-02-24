using _KIT.Event;
using Unity.Mathematics;

namespace _Game.Battle.Ecs.Events
{
    public readonly struct DamageMonsterEvent : IEvent
    {
        public readonly int Source;
        public readonly int Damage;
        public readonly float2 Position;

        public DamageMonsterEvent(int source, int damage, float2 position)
        {
            Source = source;
            Damage = damage;
            Position = position;
        }
    }
}