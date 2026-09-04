using UnityEngine;

namespace _TDS.Battle
{
    /// <summary>
    /// Đạn "đặt tại chỗ": sinh thẳng ở vị trí spawn (thường là vị trí target), KHÔNG bay.
    /// Dùng cho skill tạo vùng hiệu ứng ngay tại mục tiêu (nổ, cloud, đặt bẫy...).
    /// Detector va chạm trên prefab sẽ quét quanh vị trí này suốt totalDuration.
    /// </summary>
    public class PlaceProjectile : Projectile
    {
        // không di chuyển: base SetupAt set moving=false, Move() no-op
    }
}
