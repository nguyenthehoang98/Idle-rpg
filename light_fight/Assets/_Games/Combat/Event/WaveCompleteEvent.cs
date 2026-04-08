using _KIT.Event;

namespace _Games.Combat.Event
{
    public readonly struct WaveCompleteEvent : IEvent
    {
        public readonly int CurrentWave;
        public readonly int TotalWave;

        public WaveCompleteEvent(int currentWave, int totalWave)
        {
            CurrentWave = currentWave;
            TotalWave = totalWave;
        }
    }
}