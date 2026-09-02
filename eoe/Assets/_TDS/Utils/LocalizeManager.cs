using System;
using System.Text;
using _TDS.GameConfig;

namespace _TDS.Utils
{
    public static class LocalizeManager
    {
        private const string ATTACK_SPEED_UPGRADE = "Attack speed +{0}%";
        private const string PROJECTILE_SIZE_UPGRADE = "Project size +{0}%";
        private const string DAMAGE_PERCENTAGE_UPGRADE = "Damage +{0}%";
        private const string COOLDOWN_REDUCE_UPGRADE = "Cooldown -{0}%";
        private const string PARRALLEL_PROJECTILE_COUNT_UPGRADE = "Parrallel +{0}";
        private const string SPREAD_PROJECTILE_COUNT_UPGRADE = "Spread +{0}";
        private const string PIERCING_COUNT_UPGRADE = "Piercing +{0}";
        private const string EXPLOSIVE_RADIUS_UPGRADE = "Explosive radius +{0}";
        private const string EXPLOSIVE_DAMAGE_PERCENTAGE_UPGRADE = "Explosive damage +{0}%";
        private const string CRIT_CHANCE_UPGRADE = "Crit chance +{0}%";
        private const string CRIT_DAMAGE_UPGRADE = "Crit damage +{0}%";
        private const string KILL_INSTANT_BELOW_HEALTH_UPGRADE = "Instant kill below health +{0}%";
        
        public static StringBuilder GetUpgradeWeaponLocalize(WeaponUpgradeData data)
        {
            StringBuilder sb = new StringBuilder();
            if (data.attackRate > 0) sb.AppendLine(string.Format(ATTACK_SPEED_UPGRADE, data.attackRate * 100));
            if (data.projectileScaleBonus > 0) sb.AppendLine(string.Format(PROJECTILE_SIZE_UPGRADE, data.projectileScaleBonus * 100));
            if (data.projectileDamageMultiplier > 0) sb.AppendLine(string.Format(DAMAGE_PERCENTAGE_UPGRADE, data.projectileDamageMultiplier * 100));
            if (data.cooldownReductionPercent > 0) sb.AppendLine(string.Format(COOLDOWN_REDUCE_UPGRADE, data.cooldownReductionPercent * 100));
            if (data.projectilesPerShot > 0) sb.AppendLine(string.Format(PARRALLEL_PROJECTILE_COUNT_UPGRADE, data.projectilesPerShot));
            if (data.spreadProjectileCount > 0) sb.AppendLine(string.Format(SPREAD_PROJECTILE_COUNT_UPGRADE, data.spreadProjectileCount));
            if (data.bonusPierceCount > 0) sb.AppendLine(string.Format(PIERCING_COUNT_UPGRADE, data.bonusPierceCount));
            if (data.explosiveRadius > 0) sb.AppendLine(string.Format(EXPLOSIVE_RADIUS_UPGRADE, data.explosiveRadius));
            if (data.explosiveDamagePercent > 0) sb.AppendLine(string.Format(EXPLOSIVE_DAMAGE_PERCENTAGE_UPGRADE, data.explosiveDamagePercent * 100));
            if (data.critChance > 0) sb.AppendLine(string.Format(CRIT_CHANCE_UPGRADE, data.critChance * 100));
            if (data.critDamage > 0) sb.AppendLine(string.Format(CRIT_DAMAGE_UPGRADE, data.critDamage * 100));
            if (data.executeHealthPercent > 0) sb.AppendLine(string.Format(KILL_INSTANT_BELOW_HEALTH_UPGRADE, data.executeHealthPercent * 100));
            return sb;
        }

        public static string GetPowerLevel(int currentGroup)
        {
            if (currentGroup == 1) return "(I)";
            if (currentGroup == 2) return "(II)";
            if (currentGroup == 3) return "(III)";
            if (currentGroup == 4) return "(IV)";
            return String.Empty;
        }
    }
}