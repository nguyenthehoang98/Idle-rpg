namespace _Games.Utils
{
    public static class GlobalsPath
    {
        private const string WEAPON_SO = "{0}_Weapon_SO";
        private const string WEAPON_ITEM = "{0}_Weapon_Item";
        private const string MONSTER = "{0}_Monster";
        private const string LEVEL_DESIGN = "{0}_Level_Design";
        public const string FloatingTextDamageSpawner = "FloatingTextDamageSpawner";
        public const string RARITY_SO = "Rarity_SO";

        public static string GetWeaponSOPath(int weaponId) => string.Format(WEAPON_SO, weaponId);
        public static string GetLevelDesignPath(int levelId) => string.Format(LEVEL_DESIGN, levelId);
        public static string GetWeaponItemPath(int weaponId) => string.Format(WEAPON_ITEM, weaponId);
        public static string GetMonsterPath(int monsterId) => string.Format(MONSTER, monsterId);
    }
}