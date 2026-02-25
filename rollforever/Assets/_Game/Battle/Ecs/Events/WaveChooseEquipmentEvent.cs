using _KIT.Event;

namespace _Game.Battle.Ecs.Events
{
    public readonly struct WaveChooseEquipmentEvent : IEvent
    {
        public readonly int SlotId;
        public readonly int WeaponId;
        public readonly int Level;

        public WaveChooseEquipmentEvent(int slotId, int weaponId, int level)
        {
            SlotId = slotId;
            WeaponId = weaponId;
            Level = level;
        }
    }
}