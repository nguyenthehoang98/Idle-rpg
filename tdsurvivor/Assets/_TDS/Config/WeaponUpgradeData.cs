using System;
using UnityEngine;

namespace _TDS.Config
{
    [Serializable]
    public struct WeaponUpgradeData
    {
        public int weaponId;
        public int level;

        /// <summary>Nhóm (group) – nhiều upgrade cùng group là các lựa chọn loại trừ nhau trong card.</summary>
        public int group;

        /* ============================== MODIFIER ============================== */

        /// <summary>Tầm bắn (cộng thêm, đơn vị world).</summary>
        public float attackRange;

        /// <summary>Attack Speed (cộng dồn % – 0.1 = +10%).</summary>
        public float attackSpeed;

        /// <summary>Attack Rate (hệ số nhân).</summary>
        public float attackRate;

        /// <summary>Scale của đạn (cộng dồn).</summary>
        public float projectileScale;

        /// <summary>Sát thương đạn (cộng dồn % – 0.1 = +10% damage).</summary>
        public float projectileDamage;

        /// <summary>Giảm cooldown (% – 0.1 = -10%).</summary>
        public float cooldownReduction;

        /// <summary>Số đạn thêm (song song).</summary>
        public int projectileCount;

        /// <summary>Đạn hình quạt (spread).</summary>
        public int spreadProjectile;

        /// <summary>Số lần xuyên (pierce).</summary>
        public int pierce;

        /// <summary>Bán kính vụ nổ (cộng thêm).</summary>
        public float explosionRadius;

        /// <summary>Sát thương vụ nổ (cộng dồn %).</summary>
        public float explosionDamage;

        /// <summary>Critical rate (cộng thêm, 0..1).</summary>
        public float criticalChance;

        /// <summary>Critical damage (cộng thêm hệ số).</summary>
        public float criticalDamage;

        /// <summary>% máu mục tiêu mà weapon có thể Execute (tiêu diệt tức thì).</summary>
        public float executeHealthPercent;

        /* ============================== POWER (X2 / X3) ============================== */

        /// <summary>
        /// Nếu &gt; 0 thì đây là upgrade cấp sức mạnh: 2 = Power X2, 3 = Power X3.
        /// Dùng bởi <see cref="WeaponConfig.GetPowerLevel"/> khi weapon đạt level tương ứng.
        /// </summary>
        public int powerLevel;
    }
}