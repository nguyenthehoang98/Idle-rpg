using Unity.Mathematics;

namespace Geometry
{
    public static class GeometryCircle
    {
        public static bool Overlaps(Circle a, Circle b)
        {
            return math.lengthsq(a.center - b.center) <= (a.radius + b.radius) * (a.radius + b.radius);
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
        
        public static bool Intersect(Circle a, Circle b)
        {
            float r = a.radius + b.radius;
            
            return math.lengthsq(a.center - b.center) <= r * r;
        }
        
        public static bool Intersect(Circle c, AABB box)
        {
            float2 closest = GeometryAABB.ClosestPoint(box, c.center);

            return math.lengthsq(closest - c.center) <= c.radius * c.radius;
        }
    }
}