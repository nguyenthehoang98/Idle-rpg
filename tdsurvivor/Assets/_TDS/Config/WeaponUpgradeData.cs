using System;
using UnityEngine;

namespace _TDS.Config
{
    [Serializable]
    public struct WeaponUpgradeData
    {
        /* ============================== ĐAỊNH DANH ============================== */

        /// <summary>ID weapon / hero (hero dùng id làm weaponId).</summary>
        public int weaponId;

        /// <summary>Tên hiển thị (dùng cho row base của hero).</summary>
        public string name;

        /// <summary>Loại hero (chỉ dùng cho row base – level 0).</summary>
        public int heroType;

        public int level;

        /// <summary>Nhóm (group) – nhiều upgrade cùng group là các lựa chọn loại trừ nhau trong card.</summary>
        public int group;

        /* ============================== BASE STAT (row level 0) ============================== */

        /// <summary>Cooldown attack gốc (giây) – chỉ có ý nghĩa ở row base.</summary>
        public float attackCooldown;

        /// <summary>Sát thương gốc – chỉ có ý nghĩa ở row base.</summary>
        public int damage;

        /// <summary>Asset đạn (row base).</summary>
        public string projectileAsset;

        public int trajectoryType;

        public int hitCount;

        public float hitInterval;

        public float projectileSpeed;

        public float projectileSize;

        public float spreadAngleStep;

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
    }
}