using Geometry.Primary;
using Unity.Mathematics;

namespace Geometry
{
    /*
     * Overlaps: là check đè lên nhau
     * Intersect: kiểm tra giao nhau -> cần trả lại điểm giao nhao
     */
    public static class GeometryCircle
    {
        public static bool Overlaps(Circle a, Circle b)
        {
            float r = a.radius + b.radius;
            return math.lengthsq(a.center - b.center) <= r * r;
        }
        
        public static bool Overlaps(Circle c, AABB a)
        {
            float x = math.max(a.min.x, math.min(c.center.x, a.max.x));
            float y = math.max(a.min.y, math.min(c.center.y, a.max.y));
            float dx = x - c.center.x;
            float dy = y - c.center.y;
            return dx * dx + dy * dy <= c.radius * c.radius;
        }
        
        public static float2 ClosestPoint(Circle c, float2 p)
        {
            float2 d = p - c.center;
            float lenSq = math.lengthsq(d);
            float r = c.radius;

            if (lenSq <= float.Epsilon) return c.center + new float2(r, 0f);

            float invLen = math.rsqrt(lenSq);  
            return c.center + d * (r * invLen);
        }
    }
}