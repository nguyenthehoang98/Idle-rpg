using System;
using _KIT.Event;
using Leopotam.EcsLite;

namespace _Game.Battle.Ecs.Events
{
    public readonly struct NextWaveEvent : IEvent
    {
        public readonly EcsSystems Systems;
        public readonly Action OnCompleted;

        public NextWaveEvent(EcsSystems systems, Action onCompleted)
        {
            OnCompleted = onCompleted;
            Systems = systems;
        }
    }
}