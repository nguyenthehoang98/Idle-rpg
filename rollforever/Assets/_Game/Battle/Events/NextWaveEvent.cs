using System;
using _KIT.Event;
using Leopotam.EcsLite;

namespace _Game.Battle.Events
{
    public readonly struct NextWaveEvent : IEvent
    {
        public readonly IEcsSystems Systems;
        public readonly Action OnCompleted;

        public NextWaveEvent(IEcsSystems systems, Action onCompleted)
        {
            OnCompleted = onCompleted;
            Systems = systems;
        }
    }
}