namespace _Toolkit.Entities
{
    public readonly struct EntityChangedEvent
    {
        public readonly int EventId;
        public readonly int Entity;
        public readonly Parameter[] Parameters;

        public EntityChangedEvent(int eventId, int entity, params Parameter[] parameters)
        {
            EventId = eventId;
            Entity = entity;
            Parameters = parameters;
        }
    }
}