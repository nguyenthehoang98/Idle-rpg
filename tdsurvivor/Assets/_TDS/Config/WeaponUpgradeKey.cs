using System;

namespace _TDS.Config
{
    /// <summary>
    /// Khóa lookup <c>O(1)</c> cho một <see cref="WeaponUpgradeData"/> trong WeaponConfig.
    /// Tách riêng struct thay vì dùng <c>HashCode.Combine()</c> để kiểm soát rõ hash và tái sử dụng.
    /// </summary>
    public readonly struct WeaponUpgradeKey : IEquatable<WeaponUpgradeKey>
    {
        public int WeaponId { get; }
        public int Level { get; }
        public int Group { get; }
        public WeaponUpgradeType UpgradeType { get; }

        public WeaponUpgradeKey(int weaponId, int level, int group, WeaponUpgradeType upgradeType)
        {
            WeaponId = weaponId;
            Level = level;
            Group = group;
            UpgradeType = upgradeType;
        }

        public bool Equals(WeaponUpgradeKey other)
        {
            return WeaponId == other.WeaponId
                && Level == other.Level
                && Group == other.Group
                && UpgradeType == other.UpgradeType;
        }

        public override bool Equals(object obj)
        {
            return obj is WeaponUpgradeKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + WeaponId;
                hash = hash * 31 + Level;
                hash = hash * 31 + Group;
                hash = hash * 31 + (int)UpgradeType;
                return hash;
            }
        }

        public static bool operator ==(WeaponUpgradeKey lhs, WeaponUpgradeKey rhs) => lhs.Equals(rhs);

        public static bool operator !=(WeaponUpgradeKey lhs, WeaponUpgradeKey rhs) => !lhs.Equals(rhs);

        public override string ToString()
        {
            return $"[{UpgradeType}] weapon:{WeaponId} lvl:{Level} group:{Group}";
        }
    }
}