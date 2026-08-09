namespace _TDS.Config
{
    /// <summary>
    /// Bộ tích lũy modifier runtime – tổng hợp từ nhiều <see cref="WeaponUpgradeData"/>.
    /// Runtime dùng struct này để tính chỉ số cuối mà KHÔNG chỉnh trực tiếp dữ liệu config
    /// (tránh làm hỏng dữ liệu gốc được cache).
    /// </summary>
    public struct WeaponRuntimeModifiers
    {
        public float attackRange;
        public float attackSpeed;      // cộng dồn % (0.1 = +10%)
        public float attackRate;       // hệ số nhân
        public float projectileScale;  // cộng dồn
        public float projectileDamage; // cộng dồn % (0.1 = +10%)
        public float cooldownReduction;// cộng dồn % (0.1 = -10%)
        public int projectileCount;
        public int spreadProjectile;
        public int pierce;
        public float explosionRadius;
        public float explosionDamage;  // cộng dồn %
        public float criticalChance;
        public float criticalDamage;
        public float executeHealthPercent;

        public bool IsEmpty => projectileCount == 0
            && spreadProjectile == 0
            && pierce == 0
            && attackRange == 0f
            && attackSpeed == 0f
            && attackRate == 0f
            && projectileScale == 0f
            && projectileDamage == 0f
            && cooldownReduction == 0f
            && explosionRadius == 0f
            && explosionDamage == 0f
            && criticalChance == 0f
            && criticalDamage == 0f
            && executeHealthPercent == 0f;

        /// <summary>Reset về 0 (chuẩn bị cho một lần tích lũy mới).</summary>
        public void Reset()
        {
            this = default;
        }

        /// <summary>Tích lũy modifier từ một upgrade lên bộ tích lũy.</summary>
        public void Apply(WeaponUpgradeData data, int? stacks = null)
        {
            if (data == null) return;
            data.ApplyModifiers(ref this, stacks);
        }
    }
}