using _KIT.Event;

namespace _Games.Combat.Event
{
    public readonly struct WavePauseEvent : IEvent
    {
        public readonly bool IsGamePaused;
    }
}
