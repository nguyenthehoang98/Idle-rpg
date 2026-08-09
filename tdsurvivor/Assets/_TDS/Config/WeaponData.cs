using System;
using UnityEngine;

namespace _TDS.Config
{
    /// <summary>
    /// Dữ liệu cấu hình cơ bản của một vũ khí (một dòng trong WeaponConfig / Excel).
    /// Chứa thông tin liên kết, asset key, chỉ số base và dữ liệu runtime đã resolve.
    /// </summary>
    [Serializable]
    public struct WeaponData
    {
        /* ------------------------------ Thông tin cơ bản ------------------------------ */

        /// <summary>Weapon ID.</summary>
        public int weaponId;

        /// <summary>Tên hiển thị của vũ khí.</summary>
        public string name;

        /* ------------------------------ Liên kết với Skill ------------------------------ */

        /// <summary>ID của Skill mà vũ khí sử dụng để bắn.</summary>
        public int skillId;

        /* ------------------------------ Prefab / Asset ------------------------------ */

        /// <summary>Asset key của Prefab vũ khí (visual).</summary>
        public string prefabName;

        /// <summary>Asset key của Projectile prefab.</summary>
        public string projectileName;

        /// <summary>Asset key của Icon (Sprite).</summary>
        public string iconName;

        /// <summary>Asset key của vụ nổ (Explosion prefab).</summary>
        public string explosionName;

        /// <summary>Asset key của Attack Audio Clip.</summary>
        public string attackAudioName;

        /// <summary>Volume khi phát attack audio.</summary>
        public float attackVolume;

        /* ------------------------------ Chỉ số (Stats) ------------------------------ */

        public int attack;
        public float cooldown;
        public float attackSpeed;
        public float criticalChance;
        public float criticalDamage;

        /* ------------------------------ Runtime (resolve ở OnValidateLinkConfig, không serialize) ------------------------------ */

        /// <summary>SkillData đã resolve từ SkillConfig (thông qua skillId).</summary>
        [NonSerialized] public SkillConfigData skillData;
    }
}