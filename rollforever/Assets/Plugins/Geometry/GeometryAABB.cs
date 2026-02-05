using System;
using Unity.Mathematics;

namespace Geometry
{
    public class GeometryAABB
    {
        public static bool Overlaps(AABB a, AABB b)
        {
            return a.min.x <= b.max.x && a.max.x >= b.min.x && a.min.y <= b.max.y && a.max.y >= b.min.y;
        }
        
        public static float2 ClosestPoint(AABB aabb, float2 p)
        {
            return new float2(
                math.clamp(p.x, aabb.min.x, aabb.max.x),
                math.clamp(p.y, aabb.min.y, aabb.max.y)
            );
        }
        
        public static bool Intersect(AABB box, Circle c)
        {
            float x = math.max(box.min.x, math.min(c.center.x, box.max.x));
            float y = math.max(box.min.y, math.min(c.center.y, box.max.y));
            float dx = x - c.center.x;
            float dy = y - c.center.y;
            return dx * dx + dy * dy <= c.radius * c.radius;
        }

        public static float2 ComputeAABBNormal(float2 hitPoint, AABB box)
        {
            float left = math.abs(hitPoint.x - box.min.x);
            float right = math.abs(hitPoint.x - box.max.x);
            float bottom = math.abs(hitPoint.y - box.min.y);
            float top = math.abs(hitPoint.y - box.max.y);
            float min = math.min(math.min(left, right), math.min(bottom, top));
            if (Math.Abs(min - left) <= 0) return new float2(-1, 0);
            if (Math.Abs(min - right) <= 0) return new float2(1, 0);
            if (Math.Abs(min - bottom) <= 0) return new float2(0, -1);
            return new float2(0, 1);
        }
    }
}