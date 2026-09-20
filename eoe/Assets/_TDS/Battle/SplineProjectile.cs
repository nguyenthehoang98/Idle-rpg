using System;
using Chidwi.MinMaxSliderAttribute;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace _TDS.Battle
{
    /// <summary>
    /// Đạn bay theo quadratic Bézier từ điểm bắn đến điểm đích đã chụp lúc bắn.
    /// Tốc độ quyết định thời gian bay theo độ dài xấp xỉ của spline.
    /// </summary>
    public sealed class SplineProjectile : Projectile
    {
        [Header("Spline")]
        [SerializeField, MinMaxSlider(0f, 10f)] private Vector2 defaultArcHeight = new Vector2(0.75f, 1.5f);
        [SerializeField, Range(4, 64)] private int lengthSamples = 20;

        [Header("Gizmo Preview")]
        [SerializeField] private Vector3 gizmoTestStart = new Vector3(-2f, 0f, 0f);
        [SerializeField] private Vector3 gizmoTestEnd = new Vector3(3f, 0f, 0f);

        private const float CollisionGraceDuration = 1f / 30f;

        private Vector3 start;
        private Vector3 control;
        private Vector3 end;
        private float travelDuration;
        private bool splineInitialized;
        private TrailRenderer trail;

        private void Awake()
        {
            trail = GetComponentInChildren<TrailRenderer>();
            if (trail != null)
            {
                // Projectile là 2D: ribbon nằm trong mặt phẳng XY và xoay theo local Z.
                trail.alignment = LineAlignment.TransformZ;
                trail.textureMode = LineTextureMode.Stretch;
                trail.minVertexDistance = 0.01f;
                trail.numCornerVertices = 2;
                trail.numCapVertices = 2;
                trail.emitting = false;
            }

            OnFinished += StopTrail;
        }

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
            return SetupSpline(from, to, projectileSpeed, defaultArcHeight.x, defaultArcHeight.y);
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
            float arcHeight = UnityEngine.Random.Range(minHeight, maxHeight);
            control = ControlPoint(start, end, arcHeight);
            splineInitialized = true;

            float pathLength = ApproximateLength(start, control, end, lengthSamples);
            float f = Mathf.Max(0.01f, projectileSpeed);
            travelDuration = Mathf.Max(0.01f, pathLength / f);

            // Giữ đạn ở đích thêm 1 tick để collision runner kịp quét overlap.
            base.Setup(from, to - from, f, travelDuration + CollisionGraceDuration);
            return TotalDuration;
        }

        protected override void OnSetup(Vector3 from)
        {
            if (trail == null) trail = GetComponentInChildren<TrailRenderer>();
            if (trail == null) return;

            trail.Clear();
            trail.emitting = true;
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
            int samples = Mathf.Max(4, lengthSamples);

            // Đường thẳng tham chiếu để thấy rõ start -> end.
            Gizmos.color = Color.gray;
            Gizmos.DrawLine(gizmoStart, gizmoEnd);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(gizmoStart, 0.12f);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(gizmoEnd, 0.12f);

            if (splineInitialized)
            {
                DrawGizmoCurve(gizmoStart, control, gizmoEnd, samples, Color.yellow);
                DrawGizmoControl(gizmoStart, control, gizmoEnd);
                return;
            }

            // Preview cả hai biên của khoảng random Min/Max.
            Vector3 minControl = ControlPoint(gizmoStart, gizmoEnd, defaultArcHeight.x);
            Vector3 maxControl = ControlPoint(gizmoStart, gizmoEnd, defaultArcHeight.y);
            DrawGizmoCurve(gizmoStart, minControl, gizmoEnd, samples, Color.cyan);
            DrawGizmoCurve(gizmoStart, maxControl, gizmoEnd, samples, Color.yellow);
            DrawGizmoControl(gizmoStart, minControl, gizmoEnd);
            DrawGizmoControl(gizmoStart, maxControl, gizmoEnd);
        }

        private static void DrawGizmoControl(Vector3 from, Vector3 controlPoint, Vector3 to)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(from, controlPoint);
            Gizmos.DrawLine(controlPoint, to);
            Gizmos.DrawSphere(controlPoint, 0.12f);
        }

        private static void DrawGizmoCurve(
            Vector3 from,
            Vector3 controlPoint,
            Vector3 to,
            int samples,
            Color color)
        {
            Gizmos.color = color;
            Vector3 previous = from;
            for (int i = 1; i <= samples; i++)
            {
                Vector3 current = Evaluate(from, controlPoint, to, i / (float)samples);
                Gizmos.DrawLine(previous, current);
                previous = current;
            }
        }

        private void StopTrail()
        {
            if (trail == null) return;

            trail.emitting = false;
            trail.Clear();
        }

        protected override void OnDisable()
        {
            splineInitialized = false;
            StopTrail();
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
