using Unity.Mathematics;
using UnityEngine;

namespace Geometry
{
    public static class GeometryGizmos
    {
        // ===================== RAY =====================
        public static void DrawRay(Ray ray, Color color, float dt, float length = 10)
        {
            Debug.DrawLine(ToV3(ray.origin), ToV3(ray.origin + ray.dir * length), color, dt);
        }

        // ===================== AABB =====================
        public static void DrawAABB(AABB box, Color color, float dt)
        {
            float3 min = ToV3(box.min);
            float3 max = ToV3(box.max);

            float3 a = new float3(min.x, min.y, 0);
            float3 b = new float3(max.x, min.y, 0);
            float3 c = new float3(max.x, max.y, 0);
            float3 d = new float3(min.x, max.y, 0);

            Debug.DrawLine(a, b, color, dt);
            Debug.DrawLine(b, c, color, dt);
            Debug.DrawLine(c, d, color, dt);
            Debug.DrawLine(d, a, color, dt);
        }

        // ===================== CIRCLE =====================
        public static void DrawCircle(Circle c, Color color, float dt, int segments = 32)
        {
            float angleStep = math.PI * 2f / segments;
            float2 prev = c.center + new float2(c.radius, 0);

            for (int i = 1; i <= segments; i++)
            {
                float angle = i * angleStep;
                float2 next = c.center + new float2(
                                  math.cos(angle),
                                  math.sin(angle)
                              ) * c.radius;

                Debug.DrawLine(ToV3(prev), ToV3(next), color, dt);
                prev = next;
            }
        }

        // ===================== HIT =====================
        public static void DrawHit(RayHit2D hit, Color color, float dt, float normalLength = 0.5f)
        {
            if (!hit.hit) return;

            const float s = 0.5f;

            float3 p = ToV3(hit.point);
            Debug.DrawLine(p + new float3(-s, -s, 0), p + new float3(s, s, 0), color, dt);
            Debug.DrawLine(p + new float3(-s, s, 0), p + new float3(s, -s, 0), color, dt);
            Debug.DrawLine(p, ToV3(hit.point + hit.normal * normalLength), color, dt);
        }

        // ===================== UTILS =====================
        private static float3 ToV3(float2 v)
        {
            return new float3(v.x, v.y, 0);
        }
    }
}