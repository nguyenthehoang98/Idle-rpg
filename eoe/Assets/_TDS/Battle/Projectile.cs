using _GameToolkit.ResourceManagement;
using UnityEngine;

namespace _TDS.Battle
{
    /// <summary>
    /// Projectile bắn thẳng từ điểm A (hero) tới điểm B (target), tự huỷ khi tới nơi.
    /// Va chạm monster được xử lý bởi CollisionDetector gắn cùng prefab.
    /// </summary>
    public class Projectile : MonoBehaviour
    {
        private Vector3 from;
        private Vector3 to;
        private float speed;
        private float elapsed;
        private bool running;

        public void Setup(Vector3 from, Vector3 to, float speed)
        {
            this.from = from;
            this.to = to;
            this.speed = Mathf.Max(0.01f, speed);
            elapsed = 0;
            running = true;
            transform.position = from;
        }

        private void Update()
        {
            if (!running) return;

            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed * speed / Mathf.Max(0.001f, Vector3.Distance(from, to)));

            transform.position = Vector3.Lerp(from, to, t);

            if (t >= 1f)
            {
                running = false;
                Pool.Destroy(gameObject);
            }
        }

        /// <summary>Dừng bay + tự huỷ (gọi khi đã trúng đích trước khi bay hết quãng đường)</summary>
        public void DestroySelf()
        {
            if (!running) return;
            running = false;
            Pool.Destroy(gameObject);
        }
    }
}
