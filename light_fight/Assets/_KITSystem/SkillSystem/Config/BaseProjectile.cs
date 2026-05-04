using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _KITSystem.SkillSystem.Config
{
    [Serializable]
    public abstract class BaseProjectile
    {
        [TitleGroup("Projectile : Damage"), Indent]
        public float damageScale = 1;

        [Indent] public bool isHpPercent = false;
        [Indent] public DamageTickerType damageTickerType = DamageTickerType.None;

        [HideIf("damageTickerType", DamageTickerType.None), Indent]
        public float damageTickerIntervalInSeconds = 1f;

        [TitleGroup("Projectile : Collision"), Indent]
        [Tooltip("Giới hạn va chạm của viên đạn, nếu đủ số lần thì đạn sẽ tự hủy")]
        public int maximumCollision = 1;

        [Tooltip(
            "Ngưỡng thời gian viên đạn có thể va chạm với 1 Object lần nữa. \n(Ví dụ bãi độc gây sát thương mỗi 0.3s nếu đứng trên nó)")]
        public float collisionResetIntervalInSeconds = 1 / 30f;

        public abstract ProjectileType Type { get; }

        public enum ProjectileType
        {
            Melee,
            Ranger
        }
    }
}