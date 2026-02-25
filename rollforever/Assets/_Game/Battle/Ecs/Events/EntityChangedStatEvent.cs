using _KIT.Event;

namespace _Game.Battle.Ecs.Events
{
    public readonly struct EntityChangedStatEvent : IEvent
    {
        public readonly int Entity;

        public EntityChangedStatEvent(int entity)
        {
            Entity = entity;
        }
    }
}