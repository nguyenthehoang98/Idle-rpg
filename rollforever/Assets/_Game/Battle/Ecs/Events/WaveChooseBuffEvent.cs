using _Game.Scripts.Configs;
using _KIT.Event;

namespace _Game.Battle.Ecs.Events
{
    public readonly struct WaveChooseBuffEvent : IEvent
    {
        public readonly BuffConfig.BuffData[] Buffs;

        public WaveChooseBuffEvent(BuffConfig.BuffData[] buffs)
        {
            Buffs = buffs;
        }
    }
}