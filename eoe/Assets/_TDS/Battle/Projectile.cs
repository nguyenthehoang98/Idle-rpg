using _GameToolkit.ResourceManagement;
using UnityEngine;

namespace _TDS.Battle
{
    /// <summary>
    /// Projectile bay thẳng đều theo hướng (velocity), tự huỷ sau totalDuration.
    /// Va chạm monster được xử lý bởi CollisionDetector gắn cùng prefab.
    /// </summary>
    public class Projectile : MonoBehaviour
    {
        private Vector3 direction;
        private float speed;
        private float totalDuration;
        private float elapsed;
        private bool running;
        private bool destroyed;

        public void Setup(Vector3 from, Vector3 direction, float speed, float totalDuration)
        {
            this.direction = direction.sqrMagnitude > 1e-6f ? direction.normalized : Vector3.right;
            this.speed = Mathf.Max(0.01f, speed);
            this.totalDuration = Mathf.Max(0.01f, totalDuration);
            elapsed = 0;
            running = true;
            destroyed = false;
            transform.position = from;

            // xoay theo hướng bay (2D)
            float angle = Mathf.Atan2(this.direction.y, this.direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        /// <summary>Dừng bay tại chỗ (dành cho DOT: giữ nguyên vị trí để detector còn overlap).</summary>
        public void StopMotion()
        {
            running = false;
        }

        public void DestroySelf()
        {
            if (destroyed) return;
            destroyed = true;
            running = false;
            Pool.Destroy(gameObject);
        }

        private void Update()
        {
            if (!running) return;

            elapsed += Time.deltaTime;

            transform.position += direction * (speed * Time.deltaTime);

            if (elapsed >= totalDuration) DestroySelf();
        }
    }
}
