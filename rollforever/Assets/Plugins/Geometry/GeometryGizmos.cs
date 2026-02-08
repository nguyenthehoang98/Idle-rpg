using Geometry.Primary;
using Unity.Mathematics;
using UnityEngine;
using Ray = Geometry.Primary.Ray;

namespace Geometry
{
    public static class GeometryGizmos
    {
        // ===================== RAY =====================
        public static void DrawRay(Ray ray, Color color, float dt, float length = 10)
        {
            Debug.DrawRay(ToV3(ray.origin), ToV3(ray.dir * length), color, dt);
        }
        
        // ===================== OBB =====================
        public static void DrawPolygon(in Polygon poly, Color color, float dt)
        {
            for (int i = 0; i < poly.count; i++)
            {
                float2 a = poly.GetWorldPoint(i);
                float2 b = poly.GetWorldPoint((i + 1) % poly.count);
                Debug.DrawLine((Vector2)a, (Vector2)b, color, dt);
            }
        }
        
        // ===================== OBB =====================
        public static void DrawOBB(in OBB obb, Color color, float dt)
        {
            GeometryOBB.GetCorners(obb, out var c0, out var c1, out var c2, out var c3);

            Debug.DrawLine((Vector2)c0, (Vector2)c1, color, dt);
            Debug.DrawLine((Vector2)c1, (Vector2)c2, color, dt);
            Debug.DrawLine((Vector2)c2, (Vector2)c3, color, dt);
            Debug.DrawLine((Vector2)c3, (Vector2)c0, color, dt);
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

        // ===================== UTILS =====================
        private static float3 ToV3(float2 v)
        {
            return new float3(v.x, v.y, 0);
        }
    }
}