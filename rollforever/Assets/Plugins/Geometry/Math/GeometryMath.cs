using Unity.Mathematics;
using UnityEngine;

namespace Geometry.Math
{
    internal static partial class GeometryMath
    {
        internal const float EPSILON = 1e-6f;
    }
    
    // circle calculator
    internal static partial class GeometryMath
    {
        /// <summary>
        /// Kiểm tra điểm (x:y) có nằm trên đường viền của circle không
        /// </summary>
        internal static bool IsPointOnCircumference(float2 point, float2 center, float radius)
        {
            // (x−c.x)^2 + (y−c.y)^2 = c.r*c.r
            float2 d = point - center;
            float distSq = math.dot(d, d);
            float rSq = radius * radius;
            return math.abs(distSq - rSq) <= EPSILON;
        }

        /// <summary>
        /// Kiểm tra điểm (x:y) có nằm trong hình tròn ko
        /// </summary>
        internal static bool IsPointInsideCircle(float2 point, float2 center, float radius)
        {
            // |P - C|^2 <= r^2
            float distSq = math.lengthsq(point - center);
            float rSq = radius * radius;
            return distSq <= rSq;
        }
    }
    
    // ray calculator
    // Mục tiêu bắn ray tới các shape và kiểm tra giao nhau
    // Thực chất là giải phương trình bậc 2.
    internal static partial class GeometryMath
    {
        internal static bool RayCircle(float2 rayPoint, float2 rayDir, float rayLength,
            float2 circleCenter, float circleRadius, out float dist, out float2 point)
        {
            dist = 0;
            point = float2.zero;

            float2 o = rayPoint;
            float2 d = rayDir;
            float2 p = circleCenter;
            float r = circleRadius;
            float l = rayLength;
            
            float len = math.length(rayDir);
            if (len <= math.EPSILON)
            {
                /*Debug.Log($"[1]: rayPoint={(Vector2)o}, rayDir={(Vector2)d}, rayLength={rayLength}," +
                          $"circleCenter={(Vector2)p}, circleRadius={r}");*/
                return false;
            }

            float2 m = o - p;
            float b = math.dot(d, m);
            float c = math.dot(m, m) - r * r;

            if (c > 0f && b > 0f)
            {
                /*Debug.Log($"[2]: rayPoint={(Vector2)o}, rayDir={(Vector2)d}, rayLength={rayLength}," +
                          $"circleCenter={(Vector2)p}, circleRadius={r}," +
                          $"b={b}, c={c}");*/
                return false;
            }

            float discr = b * b - c;
            if (discr < 0f)
            {
                /*Debug.Log($"[3]: rayPoint={(Vector2)o}, rayDir={(Vector2)d}, rayLength={rayLength}," +
                          $"circleCenter={(Vector2)p}, circleRadius={r}," +
                          $"b={b}, c={c}");*/
                return false;
            }
            
            float hitT = -b - math.sqrt(discr);
            if (hitT < 0f)
                hitT = 0f;

            if (hitT > l)
            {
                return false;
            }

            dist = hitT;
            point = rayPoint + d * dist;
            return true;
        }
    }
}