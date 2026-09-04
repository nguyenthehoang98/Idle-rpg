using UnityEngine;

namespace _TDS.Battle
{
    /// <summary>
    /// Đạn bay cong: hướng xoay dần mỗi frame theo turnRate (độ/giây).
    /// prefab gắn component này (thay/thêm bên cạnh Projectile) là đạn tự bay cong,
    /// Skill không cần biết.
    /// </summary>
    public class CurveProjectile : Projectile
    {
        [Tooltip("Tốc độ xoay hướng bay, độ/giây. >0 xoay phải (thuận kim đồng hồ), <0 xoay trái.")]
        public float turnRate = 120f;

        protected override void UpdateMotion(float dt)
        {
            if (Mathf.Abs(turnRate) < 0.01f) return;

            // xoay hướng bay quanh trục Z
            direction = Quaternion.Euler(0, 0, turnRate * dt) * direction;

            RotateToDirection();
        }
    }
}
