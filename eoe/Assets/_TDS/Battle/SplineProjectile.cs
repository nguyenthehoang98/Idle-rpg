using UnityEngine;

namespace _TDS.Battle
{
    /// <summary>
    /// Đạn bay theo quadratic Bézier từ điểm bắn đến điểm đích đã chụp lúc bắn.
    /// Tốc độ quyết định thời gian bay theo độ dài xấp xỉ của spline.
    /// </summary>
    public sealed class SplineProjectile : Projectile
    {
        [Header("Spline")]
        [SerializeField, Min(0f)] private float defaultArcHeightMin = 0.75f;
        [SerializeField, Min(0f)] private float defaultArcHeightMax = 1.5f;
        [SerializeField, Range(4, 64)] private int lengthSamples = 20;

        [Header("Gizmo Preview")]
        [SerializeField] private Vector3 gizmoTestStart = new Vector3(-2f, 0f, 0f);
        [SerializeField] private Vector3 gizmoTestEnd = new Vector3(3f, 0f, 0f);
        [SerializeField, Min(0f)] private float gizmoTestArcHeight = 1.25f;

        private const float CollisionGraceDuration = 1f / 30f;

        private Vector3 start;
        private Vector3 control;
        private Vector3 end;
        private float travelDuration;
        private bool splineInitialized;

        public static Vector3 ControlPoint(Vector3 start, Vector3 end, float arcHeight)
        {
            return (start + end) * 0.5f + Vector3.up * Mathf.Max(0f, arcHeight);
        }

        public static Vector3 Evaluate(Vector3 start, Vector3 control, Vector3 end, float t)
        {
            t = Mathf.Clamp01(t);
            float u = 1f - t;
            return u * u * start + 2f * u * t * control + t * t * end;
        }

        public float SetupSpline(Vector3 from, Vector3 to, float projectileSpeed)
        {
            return SetupSpline(from, to, projectileSpeed, defaultArcHeightMin, defaultArcHeightMax);
        }

        public float SetupSpline(
            Vector3 from,
            Vector3 to,
            float projectileSpeed,
            float arcHeightMin,
            float arcHeightMax)
        {
            start = from;
            end = to;

            float minHeight = Mathf.Max(0f, arcHeightMin);
            float maxHeight = Mathf.Max(minHeight, arcHeightMax);
            float arcHeight = Random.Range(minHeight, maxHeight);
            control = ControlPoint(start, end, arcHeight);
            splineInitialized = true;

            float pathLength = ApproximateLength(start, control, end, lengthSamples);
            float speed = Mathf.Max(0.01f, projectileSpeed);
            travelDuration = Mathf.Max(0.01f, pathLength / speed);

            // Giữ đạn ở đích thêm 1 tick để collision runner kịp quét overlap.
            base.Setup(from, to - from, speed, travelDuration + CollisionGraceDuration);
            return TotalDuration;
        }

        protected override void Move(float dt)
        {
            if (!moving) return;

            float t = Mathf.Clamp01(elapsed / travelDuration);
            Vector3 next = Evaluate(start, control, end, t);
            Vector3 delta = next - transform.position;
            transform.position = next;

            if (delta.sqrMagnitude > 1e-6f)
            {
                direction = delta.normalized;
                RotateToDirection();
            }
        }

        private void OnDrawGizmos()
        {
            Vector3 gizmoStart = splineInitialized ? start : transform.TransformPoint(gizmoTestStart);
            Vector3 gizmoEnd = splineInitialized ? end : transform.TransformPoint(gizmoTestEnd);
            Vector3 gizmoControl = splineInitialized
                ? control
                : ControlPoint(gizmoStart, gizmoEnd, gizmoTestArcHeight);
            int samples = Mathf.Max(4, lengthSamples);

            // Đường thẳng tham chiếu để thấy rõ start -> end.
            Gizmos.color = Color.gray;
            Gizmos.DrawLine(gizmoStart, gizmoEnd);

            // Hai tiếp tuyến tới control point và đường cong Bézier.
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(gizmoStart, gizmoControl);
            Gizmos.DrawLine(gizmoControl, gizmoEnd);

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(gizmoStart, 0.12f);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(gizmoEnd, 0.12f);
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(gizmoControl, 0.12f);

            Gizmos.color = Color.yellow;
            Vector3 previous = gizmoStart;
            for (int i = 1; i <= samples; i++)
            {
                Vector3 current = Evaluate(gizmoStart, gizmoControl, gizmoEnd, i / (float)samples);
                Gizmos.DrawLine(previous, current);
                previous = current;
            }
        }

        protected override void OnDisable()
        {
            splineInitialized = false;
            base.OnDisable();
        }

        private static float ApproximateLength(Vector3 start, Vector3 control, Vector3 end, int samples)
        {
            int count = Mathf.Max(1, samples);
            float length = 0f;
            Vector3 previous = start;

            for (int i = 1; i <= count; i++)
            {
                Vector3 current = Evaluate(start, control, end, i / (float)count);
                length += Vector3.Distance(previous, current);
                previous = current;
            }

            return length;
        }
    }
}
