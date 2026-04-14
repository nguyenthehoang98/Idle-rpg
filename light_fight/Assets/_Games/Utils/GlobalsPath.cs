namespace _Games.Utils
{
    public static class GlobalsPath
    {
        private const string WEAPON_SO = "weapon_so_{0}";
        private const string WEAPON_ITEM = "weapon_item_{0}";
        private const string MONSTER = "monster_{0}";
        public const string TextDamageSpawner = "TextDamageSpawner";

        public static string GetWeaponSOPath(int weaponId) => string.Format(WEAPON_SO, weaponId);
        public static string GetWeaponItemPath(int weaponId) => string.Format(WEAPON_ITEM, weaponId);
        public static string GetMonsterPath(int monsterId) => string.Format(MONSTER, monsterId);
    }
}