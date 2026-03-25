using _Games.Config;
using _KIT.Event;

namespace _Games.Combat.Event
{
    public struct BattleShowWeaponInfoEvent : IEvent
    {
        public WeaponData WeaponData;
        public int Level;

        public BattleShowWeaponInfoEvent(WeaponData weaponData, int level)
        {
            WeaponData = weaponData;
            Level = level;
        }
    }
}