using System;
using UnityEngine;

namespace _TDS.Config
{
    /// <summary>
    /// Dữ liệu của một Upgrade trong một hành trình nâng cấp của weapon.
    /// Được cache theo <see cref="WeaponUpgradeKey"/> (weaponID, level, group, upgradeType).
    ///
    /// Copy nâng cấp không được sửa trực tiếp ở Runtime – hãy tích lũy vào
    /// <see cref="WeaponRuntimeModifiers"/> qua <see cref="ApplyModifiers"/>.
    /// </summary>
    [Serializable]
    public class WeaponUpgradeData
    {
        /* ============================== KEY ============================== */

        /// <summary>Weapon ID mà upgrade này thuộc về.</summary>
        public int weaponId;

        /// <summary>Level mà upgrade này mở khóa / áp dụng.</summary>
        public int level;

        /// <summary>Nhóm (group) – nhiều upgrade cùng group là các lựa chọn loại trừ nhau trong card.</summary>
        public int group;

        /// <summary>Loại upgrade.</summary>
        public WeaponUpgradeType upgradeType;

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

        /* ============================== RUNTIME STACK ============================== */

        /// <summary>Số lần upgrade này đã được áp dụng (được chọn trong Card / nâng cấp). Không serialize.</summary>
        [NonSerialized] private int stack;

        public int Stack => stack;

        /// <summary>Số lần áp dụng hiệu lực tối thiểu = 1 (bản gốc) + số stack đã chọn.</summary>
        public int ApplyCount => stack + 1;

        /// <summary>Tăng stack – dùng khi chọn/nâng upgrade này.</summary>
        public void Increase() => stack++;

        /// <summary>Giảm stack – dùng khi hoàn tác upgrade.</summary>
        public void Decrease() => stack = Mathf.Max(0, stack - 1);

        /// <summary>Reset stack về 0.</summary>
        public void ResetStack() => stack = 0;

        /// <summary>
        /// Tích lũy các modifier của bản thân lên <paramref name="mods"/>.
        /// </summary>
        /// <param name="mods">Bộ tích lũy modifier runtime.</param>
        /// <param name="stacks">Số lần áp dụng (mặc định dùng <see cref="ApplyCount"/>).</param>
        public void ApplyModifiers(ref WeaponRuntimeModifiers mods, int? stacks = null)
        {
            int n = stacks ?? ApplyCount;

            mods.attackRange += attackRange * n;
            mods.attackSpeed += attackSpeed * n;
            mods.attackRate += attackRate * n;
            mods.projectileScale += projectileScale * n;
            mods.projectileDamage += projectileDamage * n;
            mods.cooldownReduction += cooldownReduction * n;
            mods.projectileCount += projectileCount * n;
            mods.spreadProjectile += spreadProjectile * n;
            mods.pierce += pierce * n;
            mods.explosionRadius += explosionRadius * n;
            mods.explosionDamage += explosionDamage * n;
            mods.criticalChance += criticalChance * n;
            mods.criticalDamage += criticalDamage * n;
            mods.executeHealthPercent += executeHealthPercent * n;
        }
    }
}