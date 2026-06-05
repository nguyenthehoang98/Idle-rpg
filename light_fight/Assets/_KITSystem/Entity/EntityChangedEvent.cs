namespace _KITSystem.Entity
{
    public readonly struct EntityChangedEvent
    {
        public readonly int EventId;
        public readonly int Entity;
        public readonly ParameterValue[] Values;

        public EntityChangedEvent(int eventId, int entity, params ParameterValue[] values)
        {
            EventId = eventId;
            Entity = entity;
            Values = values;
        }
    }
}