namespace _Games.Utils
{
    public static class GlobalsPath
    {
        private const string WEAPON_SO = "Weapon_SO_{0}";
        private const string WEAPON_ITEM = "Weapon_Item_{0}";
        private const string MONSTER = "Monster_{0}";
        public const string TextDamageSpawner = "TextDamageSpawner";
        public const string RARITY_SO = "Rarity_SO";

        public static string GetWeaponSOPath(int weaponId) => string.Format(WEAPON_SO, weaponId);
        public static string GetWeaponItemPath(int weaponId) => string.Format(WEAPON_ITEM, weaponId);
        public static string GetMonsterPath(int monsterId) => string.Format(MONSTER, monsterId);
    }
}