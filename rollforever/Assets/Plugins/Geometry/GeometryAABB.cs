using System;
using Unity.Mathematics;

namespace Geometry
{
    /*
     * Overlaps: là check đè lên nhau
     * Intersect: kiểm tra giao nhau -> cần trả lại điểm giao nhao
     */
    public static class GeometryAABB
    {
        public static bool Contains(AABB box, float2 p)
        {
            return p.x >= box.min.x && 
                   p.x <= box.max.x &&
                   p.y >= box.min.y && 
                   p.y <= box.max.y;
        }
        
        public static bool Overlaps(AABB a, AABB b)
        {
            return a.min.x <= b.max.x && 
                   a.max.x >= b.min.x && 
                   a.min.y <= b.max.y && 
                   a.max.y >= b.min.y;
        }
        
        public static bool Overlaps(AABB a, Circle b)
        {
            float x = math.max(a.min.x, math.min(b.center.x, a.max.x));
            float y = math.max(a.min.y, math.min(b.center.y, a.max.y));
            float dx = x - b.center.x;
            float dy = y - b.center.y;
            return dx * dx + dy * dy <= b.radius * b.radius;
        }
        
        public static float2 ClosestPoint(AABB a, float2 p)
        {
            return new float2(
                math.clamp(p.x, a.min.x, a.max.x),
                math.clamp(p.y, a.min.y, a.max.y)
            );
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