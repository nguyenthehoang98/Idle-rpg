using System.Text;
using _Game.Configs;

namespace _Game.Utils
{
    public static class LocalizeManager
    {
        private const string ATTACK_SPEED_UPGRADE = "Attack speed +{0}%";
        private const string PROJECTILE_SIZE_UPGRADE = "Project size +{0}%";
        private const string DAMAGE_PERCENTAGE_UPGRADE = "Damage +{0}%";
        private const string COOLDOWN_REDUCE_UPGRADE = "Cooldown -{0}%";
        private const string PARRALLEL_PROJECTILE_COUNT_UPGRADE = "Parrallel +{0}";
        private const string PARRALLEL_PROJECT_DAMAGE_PERCENTAGE_UPGRADE = "Parrallel damage +{0}%";
        private const string SPREAD_PROJECTILE_COUNT_UPGRADE = "Spread +{0}";
        private const string SPREAD_PROJECT_DAMAGE_PERCENTAGE_UPGRADE = "Spread damage +{0}%";
        private const string PIERCING_COUNT_UPGRADE = "Piercing +{0}";
        private const string EXPLOSIVE_RADIUS_UPGRADE = "Explosive radius +{0}";
        private const string EXPLOSIVE_DAMAGE_PERCENTAGE_UPGRADE = "Explosive damage +{0}%";
        private const string CRIT_CHANCE_UPGRADE = "Crit chance +{0}%";
        private const string CRIT_DAMAGE_UPGRADE = "Crit damage +{0}%";
        private const string BOUNCE_COUNT_UPGRADE = "Bounce +{0}";
        private const string BOUNCE_DAMAGE_PERCENT_UPGRADE = "Bounce damage +{0}%";
        private const string KILL_INSTANT_BELOW_HEALTH_UPGRADE = "Instant kill below health +{0}%";
        
        public static StringBuilder GetUpgradeWeaponLocalize(WeaponUpgradeData data)
        {
            StringBuilder sb = new StringBuilder();
            if (data.attackSpeed > 0) sb.AppendLine(string.Format(ATTACK_SPEED_UPGRADE, data.attackSpeed * 100));
            if (data.projectileSize > 0) sb.AppendLine(string.Format(PROJECTILE_SIZE_UPGRADE, data.projectileSize * 100));
            if (data.damagePercent > 0) sb.AppendLine(string.Format(DAMAGE_PERCENTAGE_UPGRADE, data.damagePercent * 100));
            if (data.cooldownReduce > 0) sb.AppendLine(string.Format(COOLDOWN_REDUCE_UPGRADE, data.cooldownReduce * 100));
            if (data.parallelCount > 0) sb.AppendLine(string.Format(PARRALLEL_PROJECTILE_COUNT_UPGRADE, data.parallelCount));
            if (data.spreadCount > 0) sb.AppendLine(string.Format(SPREAD_PROJECTILE_COUNT_UPGRADE, data.spreadCount));
            if (data.spreadDamagePercent > 0) sb.AppendLine(string.Format(SPREAD_PROJECT_DAMAGE_PERCENTAGE_UPGRADE, data.spreadDamagePercent * 100));
            if (data.piercingCount > 0) sb.AppendLine(string.Format(PIERCING_COUNT_UPGRADE, data.piercingCount));
            if (data.explosiveRadius > 0) sb.AppendLine(string.Format(EXPLOSIVE_RADIUS_UPGRADE, data.explosiveRadius));
            if (data.explosiveDamagePercent > 0) sb.AppendLine(string.Format(EXPLOSIVE_DAMAGE_PERCENTAGE_UPGRADE, data.explosiveDamagePercent * 100));
            if (data.critChance > 0) sb.AppendLine(string.Format(CRIT_CHANCE_UPGRADE, data.critChance * 100));
            if (data.critDamage > 0) sb.AppendLine(string.Format(CRIT_DAMAGE_UPGRADE, data.critDamage * 100));
            if (data.bounceCount > 0) sb.AppendLine(string.Format(BOUNCE_COUNT_UPGRADE, data.bounceCount));
            if (data.bounceDamagePercent > 0) sb.AppendLine(string.Format(BOUNCE_DAMAGE_PERCENT_UPGRADE, data.bounceDamagePercent * 100));
            if (data.killInstantBelowHealthPercent > 0) sb.AppendLine(string.Format(KILL_INSTANT_BELOW_HEALTH_UPGRADE, data.killInstantBelowHealthPercent * 100));
            return sb;
        }
    }
}