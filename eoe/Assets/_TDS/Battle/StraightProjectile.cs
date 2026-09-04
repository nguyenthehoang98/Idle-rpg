using UnityEngine;

namespace _TDS.Battle
{
    /// <summary>
    /// Đạn bay thẳng đều (hành vi Projectile base trước đây). Dùng cho prefab đạn thường
    /// không có quỹ đạo đặc biệt. Projectile base giờ abstract nên prefab phải gắn subclass này.
    /// </summary>
    public class StraightProjectile : Projectile
    {
        // không cần override: base đã bay thẳng theo direction + speed
    }
}
