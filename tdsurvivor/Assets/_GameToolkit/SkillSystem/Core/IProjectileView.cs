using UnityEngine;

namespace _GameToolkit.SkillSystem.Core
{
    /// <summary>
    /// Bridge interface giữa SkillSystem (pure logic) và visual representation của game.
    /// Mỗi game tự implement interface này bằng MonoBehaviour/prefab của riêng mình
    /// (thường là Projectile component).
    /// </summary>
    public interface IProjectileView
    {
        /// <summary>
        /// Được gọi mỗi tick bởi CastProjectileAction để cập nhật vị trí + hướng visual.
        /// </summary>
        void SetPosition(Vector2 position, Vector2 direction, float deltaTime);

        /// <summary>
        /// Được gọi khi action kết thúc (EndLifeCycle/Interrupt) — game tự xử lý
        /// pool return / destroy tại đây.
        /// </summary>
        void Destroy();
    }
}
