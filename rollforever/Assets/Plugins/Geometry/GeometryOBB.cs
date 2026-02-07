using Geometry.Primary;
using Unity.Mathematics;

namespace Geometry
{
    public static class GeometryOBB
    {
        public static bool Contains(in OBB obb, float2 point)
        {
            float2 d = point - obb.center;

            float px = math.dot(d, obb.axisX);
            if (math.abs(px) > obb.halfSize.x) return false;

            float py = math.dot(d, obb.axisY);
            if (math.abs(py) > obb.halfSize.y) return false;

            return true;
        }
        
        public static bool Overlaps(in OBB obb, in AABB aabb)
        {
            OBB aabbAsOBB = new OBB(
                (aabb.min + aabb.max) * 0.5f,
                (aabb.max - aabb.min) * 0.5f,
                new float2(1, 0),
                new float2(0, 1)
            );

            return Overlaps(obb, aabbAsOBB);
        }
        
        public static bool Overlaps(in OBB obb, in Circle circle)
        {
            // Axis 1 & 2: OBB axes
            if (!OverlapOnAxis(obb, circle, obb.axisX)) return false;
            if (!OverlapOnAxis(obb, circle, obb.axisY)) return false;

            // Axis 3: từ closest point → circle center
            float2 closest = ClosestPoint(obb, circle.center);
            float2 axis = circle.center - closest;

            // Nếu tâm circle nằm trong OBB → overlap chắc chắn
            if (math.lengthsq(axis) < 1e-6f)
                return true;

            axis = math.normalize(axis);
            return OverlapOnAxis(obb, circle, axis);
        }
        
        public static float2 ClosestPoint(in OBB obb, float2 point)
        {
            float2 d = point - obb.center;
            float2 result = obb.center;

            float distX = math.dot(d, obb.axisX);
            distX = math.clamp(distX, -obb.halfSize.x, obb.halfSize.x);
            result += obb.axisX * distX;

            float distY = math.dot(d, obb.axisY);
            distY = math.clamp(distY, -obb.halfSize.y, obb.halfSize.y);
            result += obb.axisY * distY;

            return result;
        }
        
        public static void GetCorners(in OBB obb, out float2 c0, out float2 c1, out float2 c2, out float2 c3)
        {
            float2 ex = obb.axisX * obb.halfSize.x;
            float2 ey = obb.axisY * obb.halfSize.y;

            c0 = obb.center + ex + ey;
            c1 = obb.center - ex + ey;
            c2 = obb.center - ex - ey;
            c3 = obb.center + ex - ey;
        }
        
        public static bool Overlaps(in OBB a, in OBB b)
        {
            return OverlapOnAxis(a, b, a.axisX)
                   && OverlapOnAxis(a, b, a.axisY)
                   && OverlapOnAxis(a, b, b.axisX)
                   && OverlapOnAxis(a, b, b.axisY);
        }
        
        static bool OverlapOnAxis(in OBB obb, in Circle circle, float2 axis)
        {
            ProjectOBB(obb, axis, out float minA, out float maxA);

            float center = math.dot(circle.center, axis);
            float minB = center - circle.radius;
            float maxB = center + circle.radius;

            return !(minA > maxB || minB > maxA);
        }

        static bool OverlapOnAxis(in OBB a, in OBB b, float2 axis)
        {
            ProjectOBB(a, axis, out float minA, out float maxA);
            ProjectOBB(b, axis, out float minB, out float maxB);

            return !(minA > maxB || minB > maxA);
        }

        static void ProjectOBB(in OBB obb, float2 axis, out float min, out float max)
        {
            float2 ex = obb.axisX * obb.halfSize.x;
            float2 ey = obb.axisY * obb.halfSize.y;

            float r =
                math.abs(math.dot(axis, ex)) +
                math.abs(math.dot(axis, ey));

            float c = math.dot(axis, obb.center);

            min = c - r;
            max = c + r;
        }
    }
}