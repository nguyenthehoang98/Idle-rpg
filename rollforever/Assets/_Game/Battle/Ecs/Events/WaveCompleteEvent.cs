using System;
using _KIT.Event;
using Leopotam.EcsLite;

namespace _Game.Battle.Ecs.Events
{
    public readonly struct WaveCompleteEvent : IEvent
    {
        public readonly int Wave;

        public WaveCompleteEvent(int wave)
        {
            Wave = wave;
        }
    }
}