using _Toolkit.SkillSystem.Core;

namespace _TDS.GameplayScene.SkillSystem
{
    /// <summary>
    /// Runtime data khi cast skill — nạp từ Hero stats + buff, truyền vào SkillManager.
    /// </summary>
    public struct SkillContext
    {
        public int TargetEntity;
        public string AssetName;
        public ProjectileContext Projectile;
        public DamageContext Damage;
        public FireContext Fire;
        public AttackResetTiming ResetTiming;
        public int Attack;
        public float CritRate;
        public float CritDamage;
        public float ProjectileDistanceStep;
        public float ProjectileAngleStep;
        public int SpreadProjectileCount;
        public float SpreadDamageScale;
        public int ParallelProjectileCount;
        public float ParallelDamageScale;
        public string ExplosiveAssetName;
        public float ExplosiveRadius;
        public float ExplosiveDamageScale;
        public float InstantKillTargetBelowHealthPercent;

        public SkillContext(int targetEntity, string assetName, 
            ProjectileContext projectile, DamageContext damage, FireContext fire,
            AttackResetTiming resetTiming,
            int attack, float critDamage, float critRate,
            float projectileDistanceStep, float projectileAngleStep, 
            int spreadProjectileCount, float spreadDamageScale, 
            int parallelProjectileCount, float parallelDamageScale, 
            string explosiveAssetName, float explosiveRadius, float explosiveDamageScale, 
            float instantKillTargetBelowHealthPercent)
        {
            ResetTiming = resetTiming;
            TargetEntity = targetEntity;
            AssetName = assetName;
            Projectile = projectile;
            Attack = attack;
            CritRate = critRate;
            CritDamage = critDamage;
            Damage = damage;
            Fire = fire;
            ProjectileDistanceStep = projectileDistanceStep;
            ProjectileAngleStep = projectileAngleStep;
            SpreadProjectileCount = spreadProjectileCount;
            SpreadDamageScale = spreadDamageScale;
            ParallelProjectileCount = parallelProjectileCount;
            ParallelDamageScale = parallelDamageScale;
            ExplosiveAssetName = explosiveAssetName;
            ExplosiveRadius = explosiveRadius;
            ExplosiveDamageScale = explosiveDamageScale;
            InstantKillTargetBelowHealthPercent = instantKillTargetBelowHealthPercent;
        }
    }
}