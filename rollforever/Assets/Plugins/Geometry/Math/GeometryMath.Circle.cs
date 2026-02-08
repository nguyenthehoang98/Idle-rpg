using Unity.Mathematics;

namespace Geometry.Math
{
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
}