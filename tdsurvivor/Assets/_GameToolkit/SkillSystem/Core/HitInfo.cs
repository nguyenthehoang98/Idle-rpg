using UnityEngine;

namespace _GameToolkit.SkillSystem.Core
{
    /// <summary>
    /// Thông tin entity bị trúng khi projectile/collider chạm.
    /// Game-specific logic (damage, death, exp...) được xử lý bên ngoài
    /// qua callback trong CastProjectileAction.
    /// </summary>
    public readonly struct HitInfo
    {
        public readonly int EntityId;
        public readonly Vector2 Position;
        public readonly bool IsLastHit;

        public HitInfo(int entityId, Vector2 position, bool isLastHit)
        {
            EntityId = entityId;
            Position = position;
            IsLastHit = isLastHit;
        }

        /// <summary>Hit đơn giản không cần vị trí projectile (ví dụ DoT tick).</summary>
        public HitInfo(int entityId)
        {
            EntityId = entityId;
            Position = Vector2.zero;
            IsLastHit = false;
        }
    }
}
