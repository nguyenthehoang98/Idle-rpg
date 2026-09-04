using System;
using _GameToolkit.ResourceManagement;
using UnityEngine;

namespace _TDS.Battle
{
    /// <summary>
    /// Projectile base: tự quản lý vòng đời (bay, tự huỷ sau duration). Skill chỉ spawn
    /// + đăng ký va chạm, không can thiệp quỹ đạo.
    ///
    /// Quỹ đạo = hướng/tốc độ hiện tại, cập nhật mỗi frame qua UpdateMotion(dt).
    /// Subclass ghi đè UpdateMotion để có quỹ đạo riêng (cong, boomerang, đuổi theo target...)
    /// mà không cần sửa Skill/SkillFactory.
    /// </summary>
    public class Projectile : MonoBehaviour
    {
        protected Vector3 direction;
        protected float speed;
        protected float totalDuration;
        protected float elapsed;
        protected bool running;
        protected bool destroyed;

        /// <summary>Đạn bay hết duration (hoặc bị StopMotion) -> skill có thể dọn action nếu cần.</summary>
        public event Action OnFinished;

        public void Setup(Vector3 from, Vector3 direction, float speed, float totalDuration)
        {
            this.direction = direction.sqrMagnitude > 1e-6f ? direction.normalized : Vector3.right;
            this.speed = Mathf.Max(0.01f, speed);
            this.totalDuration = Mathf.Max(0.01f, totalDuration);
            elapsed = 0;
            running = true;
            destroyed = false;
            transform.position = from;

            OnSetup(from);

            RotateToDirection();
        }

        /// <summary>Hook khởi tạo cho subclass (lưu vị trí gốc, tham số quỹ đạo...).</summary>
        protected virtual void OnSetup(Vector3 from)
        {
        }

        /// <summary>Cập nhật quỹ đạo mỗi frame. Base: bay thẳng. Subclass ghi đè để đổi hướng/tốc độ.</summary>
        protected virtual void UpdateMotion(float dt)
        {
            // không làm gì: hướng + speed cố định
        }

        /// <summary>Dừng bay tại chỗ (DOT: giữ vị trí để detector còn overlap).</summary>
        public void StopMotion()
        {
            running = false;
        }

        public void DestroySelf()
        {
            if (destroyed) return;
            destroyed = true;
            running = false;
            OnFinished?.Invoke();
            Pool.Destroy(gameObject);
        }

        protected void RotateToDirection()
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        protected virtual void Update()
        {
            if (!running) return;

            UpdateMotion(Time.deltaTime);

            elapsed += Time.deltaTime;

            transform.position += direction * (speed * Time.deltaTime);

            if (elapsed >= totalDuration) DestroySelf();
        }
    }
}
