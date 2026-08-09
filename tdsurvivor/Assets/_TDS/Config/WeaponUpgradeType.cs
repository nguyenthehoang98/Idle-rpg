namespace _TDS.Config
{
    /// <summary>
    /// Loại của một WeaponUpgradeData – dùng để phân nhóm lựa chọn trong Card Upgrade
    /// và xác định modifier sẽ tác động lên loại chỉ số nào của weapon.
    /// </summary>
    public enum WeaponUpgradeType
    {
        None = 0,

        /// <summary>Power X2 / X3 – upgrade dùng để nâng cấp sức mạnh theo level.</summary>
        Power,

        /// <summary>Sát thương (damage / projectile damage).</summary>
        Damage,

        /// <summary>Attack Speed – tăng tốc độ bắn.</summary>
        AttackSpeed,

        /// <summary>Attack Rate – tỉ lệ tấn công.</summary>
        AttackRate,

        /// <summary>Tầm bắn (attack range).</summary>
        Range,

        /// <summary>Thuộc tính đạn (scale / tốc độ / đếm đạn).</summary>
        Projectile,

        /// <summary>Đạn hình quạt (spread).</summary>
        Spread,

        /// <summary>Xuyên (pierce).</summary>
        Pierce,

        /// <summary>Vụ nổ (explosion).</summary>
        Explosion,

        /// <summary>Chí mạng (critical).</summary>
        Critical,

        /// <summary>Tiêu diệt tức thì dưới % máu (execute).</summary>
        Execute,
    }
}