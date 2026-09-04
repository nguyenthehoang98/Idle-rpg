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
    /// Subclass ghi đè Move(dt) để tự đặt vị trí (lerp theo curve, đuổi target, teleport...).
    ///
    /// Abstract: prefab phải gắn subclass cụ thể (StraightProjectile/SpeedCurveProjectile/...),
    /// không gắn base trực tiếp.
    /// </summary>
    public abstract class Projectile : MonoBehaviour, ITickRunner
    {
        protected Vector3 direction;
        protected Vector3 destination;   // đích (thường là vị trí target) — subclass có thể dùng
        protected float speed;
        protected float totalDuration;
        protected float elapsed;
        protected bool moving = true;    // có di chuyển không (false = đứng yên nhưng vẫn đếm thời gian sống)
        protected bool destroyed;
        private bool registered;

        /// <summary>Đạn hết duration (hoặc bị DestroySelf) -> skill có thể dọn action nếu cần.</summary>
        public event Action OnFinished;

        /// <summary>Khởi tạo đạn bay từ `from` theo hướng `direction`.</summary>
        public void Setup(Vector3 from, Vector3 direction, float speed, float totalDuration)
        {
            this.direction = direction.sqrMagnitude > 1e-6f ? direction.normalized : Vector3.right;
            this.speed = Mathf.Max(0.01f, speed);
            this.totalDuration = Mathf.Max(0.01f, totalDuration);
            destination = from + this.direction;
            moving = true;
            Init(from);
        }

        /// <summary>Khởi tạo đạn sinh thẳng tại `spawn` (không bay), dùng cho skill đặt tại chỗ / tại target.</summary>
        public void SetupAt(Vector3 spawn, Vector3 toward, float totalDuration)
        {
            direction = toward.sqrMagnitude > 1e-6f ? (toward - spawn).normalized : Vector3.right;
            speed = 0f; // không bay
            this.totalDuration = Mathf.Max(0.01f, totalDuration);
            destination = toward;
            moving = false; // sinh tại chỗ, không di chuyển
            Init(spawn);
        }

        private void Init(Vector3 spawn)
        {
            elapsed = 0;
            destroyed = false;
            transform.position = spawn;

            OnSetup(spawn);

            RotateToDirection();

            RegisterTick();
        }

        /// <summary>Hook khởi tạo cho subclass (lưu vị trí gốc, tham số quỹ đạo...).</summary>
        protected virtual void OnSetup(Vector3 from)
        {
        }

        /// <summary>Cập nhật quỹ đạo mỗi tick (xoay hướng, tham số...). Base: không làm gì.</summary>
        protected virtual void UpdateMotion(float dt)
        {
        }

        /// <summary>Di chuyển 1 bước mỗi tick. Base: tiến thẳng theo direction*speed nếu moving.
        /// Subclass ghi đè để tự đặt vị trí (lerp theo curve, đuổi target, teleport...).</summary>
        protected virtual void Move(float dt)
        {
            if (!moving) return;

            transform.position += direction * (speed * dt);
        }

        /// <summary>Được ProjectileTickRunner gọi mỗi tick 30Hz (đồng bộ với va chạm).</summary>
        public void Tick(float deltaTime)
        {
            if (destroyed) return;

            UpdateMotion(deltaTime);

            elapsed += deltaTime;

            Move(deltaTime);

            if (elapsed >= totalDuration) DestroySelf();
        }

        /// <summary>Dừng di chuyển tại chỗ (DOT: giữ vị trí để detector còn overlap), vẫn đếm thời gian sống.</summary>
        public void StopMotion()
        {
            moving = false;
        }

        public void DestroySelf()
        {
            if (destroyed) return;
            destroyed = true;
            moving = false;
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
