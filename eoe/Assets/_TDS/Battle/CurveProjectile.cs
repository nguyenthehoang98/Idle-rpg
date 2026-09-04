using UnityEngine;

namespace _TDS.Battle
{
    /// <summary>
    /// Đạn bay theo AnimationCurve (chỉnh trong inspector): curve quy định độ lệch NGANG
    /// (vuông góc với hướng bay) tại từng thời điểm trong projectileDuration.
    ///
    /// - Trục X của curve = tiến trình thời gian 0..1 (0 = vừa bắn, 1 = hết duration)
    /// - Trục Y của curve = độ lệch ngang (đơn vị world unit; âm = lệch trái, dương = phải)
    ///
    /// Đạn vẫn tiến thẳng theo hướng bắn với speed, chỉ vị trí ngang được curve điều khiển
    /// -> tạo đường bay cong (parabol, zigzag, lượn sóng...) mà Skill không cần biết.
    /// </summary>
    public class CurveProjectile : Projectile
    {
        [Tooltip("Độ lệch ngang (vuông góc hướng bay) theo tiến trình 0..1. Y âm = trái, dương = phải.")]
        public AnimationCurve lateralCurve = AnimationCurve.Linear(0f, 0f, 1f, 0f);

        private Vector3 startPosition;
        private Vector3 lateralDir;      // hướng vuông góc (trái/phải) với hướng bay ban đầu

        protected override void OnSetup(Vector3 from)
        {
            startPosition = from;
            // vuông góc trái với hướng bay (xoay -90 độ)
            lateralDir = Quaternion.Euler(0, 0, -90f) * direction;
        }

        protected override void Move(float dt)
        {
            // elapsed đã được base cộng dt trước khi gọi Move -> dùng elapsed là thời điểm cuối bước
            float t = Mathf.Clamp01(elapsed / totalDuration);

            // vị trí = tiến thẳng theo hướng + lệch ngang theo curve
            Vector3 forwardPos = startPosition + direction * (speed * elapsed);
            Vector3 lateralOffset = lateralDir * lateralCurve.Evaluate(t);

            transform.position = forwardPos + lateralOffset;
        }
    }
}
