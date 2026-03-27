using _Games.Combat.Model;
using _KIT.Event;
using UnityEngine;

namespace _Games.Combat.Event
{
    public struct SpawnTextDamageEvent : IEvent
    {
        public TextDamageType Type;
        public Vector3 Position;
        public int Damage;

        public SpawnTextDamageEvent(TextDamageType type, int damage, Vector3 position)
        {
            Type = type;
            Damage = damage;
            Position = position;
        }
    }
}