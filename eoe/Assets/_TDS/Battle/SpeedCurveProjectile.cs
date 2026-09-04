using UnityEngine;

namespace _TDS.Battle
{
    /// <summary>
    /// Đạn bay với TỐC ĐỘ được AnimationCurve (inspector) điều khiển:
    /// curve quy định hệ số nhân tốc độ tại từng thời điểm trong projectileDuration.
    ///
    /// - Trục X = tiến trình thời gian 0..1 (0 = vừa bắn, 1 = hết duration)
    /// - Trục Y = speed multiplier (1 = tốc độ gốc, 0 = đứng yên, 2 = gấp đôi...)
    ///
    /// Đạn vẫn bay theo hướng bắn (direction), chỉ tốc độ được lerp theo curve
    /// -> tạo cảm giác nhanh dần / chậm dần / khựng giữa chừng mà Skill không cần biết.
    /// </summary>
    public class SpeedCurveProjectile : Projectile
    {
        [Tooltip("Hệ số nhân tốc độ bay theo tiến trình 0..1. 1 = tốc độ gốc (speed).")]
        public AnimationCurve speedCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);

        protected override void Move(float dt)
        {
            if (!moving) return; // DOT / StopMotion: đứng yên

            // elapsed đã được base cộng dt trước khi gọi Move
            float t = Mathf.Clamp01(elapsed / totalDuration);

            float multiplier = Mathf.Max(0f, speedCurve.Evaluate(t));

            transform.position += direction * (speed * multiplier * dt);
        }
    }
}
