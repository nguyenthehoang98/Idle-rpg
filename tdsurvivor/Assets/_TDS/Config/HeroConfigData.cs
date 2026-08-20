using System;

namespace _TDS.Config
{
    [Serializable]
    public struct HeroConfigData
    {
        public int id;
        public string name;
        public int heroType;
        public float attackRange;
        public float attackCooldown;
        public int damage;
        public float critRate;
        public float critDamage;
        public string projectileAsset;
        public int trajectoryType;
        public int hitCount;
        public float hitInterval;
        public float projectileSpeed;
        public float projectileSize;
        public int spreadCount;
        public float spreadAngleStep;
        public float spreadDamageScale;
        public int parallelCount;
        public float parallelDamageScale;
        public float explosiveRadius;
        public float explosiveDamageScale;
    }
}
