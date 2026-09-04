using System;
using _GameToolkit.ResourceManagement;
using _GameToolkit.Updater;
using UnityEngine;

namespace _TDS.Battle
{
    /// <summary>
    /// Projectile base: tự quản lý vòng đời (bay, tự huỷ sau duration). Skill chỉ spawn
    /// + đăng ký va chạm, không can thiệp quỹ đạo.
    ///
    /// Đạn di chuyển theo tick 30Hz (ProjectileTickRunner, cùng nhịp ColliderTickRunner)
    /// chứ KHÔNG theo frame Unity -> đồng bộ với va chạm, không xuyên monster.
    ///
    /// Quỹ đạo = hướng/tốc độ hiện tại, cập nhật mỗi tick qua UpdateMotion(dt).
    /// Subclass ghi đè UpdateMotion để có quỹ đạo riêng (cong, boomerang, đuổi target...).
    /// Abstract: prefab phải gắn subclass cụ thể (StraightProjectile/CurveProjectile/...),
    /// không gắn base trực tiếp.
    /// </summary>
    public abstract class Projectile : MonoBehaviour, ITickRunner
    {
        protected Vector3 direction;
        protected float speed;
        protected float totalDuration;
        protected float elapsed;
        protected bool running;
        protected bool destroyed;
        private bool registered;

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

            RegisterTick();
        }

        /// <summary>Hook khởi tạo cho subclass (lưu vị trí gốc, tham số quỹ đạo...).</summary>
        protected virtual void OnSetup(Vector3 from)
        {
        }

        /// <summary>Cập nhật quỹ đạo mỗi tick. Base: bay thẳng. Subclass ghi đè để đổi hướng/tốc độ.</summary>
        protected virtual void UpdateMotion(float dt)
        {
            // không làm gì: hướng + speed cố định
        }

        /// <summary>Được ProjectileTickRunner gọi mỗi tick 30Hz (đồng bộ với va chạm).</summary>
        public void Tick(float deltaTime)
        {
            if (!running) return;

            UpdateMotion(deltaTime);

            elapsed += deltaTime;

            transform.position += direction * (speed * deltaTime);

            if (elapsed >= totalDuration) DestroySelf();
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
            UnregisterTick();
            OnFinished?.Invoke();
            Pool.Destroy(gameObject);
        }

        private void RegisterTick()
        {
            if (registered || ProjectileTickRunner.Instance == null) return;
            registered = true;
            ProjectileTickRunner.Instance.Add(this);
        }

        private void UnregisterTick()
        {
            if (!registered || ProjectileTickRunner.Instance == null) return;
            registered = false;
            ProjectileTickRunner.Instance.Remove(this);
        }

        protected void RotateToDirection()
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        protected virtual void OnDisable()
        {
            // object về pool (SetActive false) -> phải rời khỏi runner để không tick object ẩn
            UnregisterTick();
        }
    }
}
