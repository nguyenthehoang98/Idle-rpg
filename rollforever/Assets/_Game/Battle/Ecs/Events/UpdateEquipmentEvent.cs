using _KIT.Event;

namespace _Game.Battle.Ecs.Events
{
    public readonly struct UpdateEquipmentEvent : IEvent
    {
        public readonly int SlotId;
        public readonly int WeaponId;
        public readonly int Level;

        public UpdateEquipmentEvent(int slotId, int weaponId, int level)
        {
            SlotId = slotId;
            WeaponId = weaponId;
            Level = level;
        }
    }
}