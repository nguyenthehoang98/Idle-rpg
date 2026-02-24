using _KIT.Event;

namespace _Game.Battle.Ecs.Events
{
    public readonly struct EquipEquipmentEvent : IEvent
    {
        public readonly int SlotId;
        public readonly int WeaponId;
        public readonly int Level;

        public EquipEquipmentEvent(int slotId, int weaponId, int level)
        {
            SlotId = slotId;
            WeaponId = weaponId;
            Level = level;
        }
    }
}