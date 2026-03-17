using _KIT.Event;

namespace _Games.Combat.Events
{
    public readonly struct WaveCompleteEvent : IEvent
    {
        public readonly int WaveIndex;

        public WaveCompleteEvent(int waveIndex)
        {
            WaveIndex = waveIndex;
        }
    }
}
