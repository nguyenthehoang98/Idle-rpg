using Geometry.Primary;
using Unity.Mathematics;

namespace Geometry.Math
{
    internal static partial class GeometryMath
    {
        internal static bool IsObbContainPoint(
            OBB obb, float2 point
        )
        {
            float2 d = point - obb.center;
            float px = math.dot(d, obb.axisX);
            if (math.abs(px) > obb.halfSize.x)
                return false;
            float py = math.dot(d, obb.axisY);
            if (math.abs(py) > obb.halfSize.y) 
                return false;
            return true;
        }
        
        internal static bool OverlapObbBox(
            OBB obb, Box box
            )
        {
            OBB aabbAsObb = new OBB(
                (box.min + box.max) * 0.5f,
                (box.max - box.min) * 0.5f,
                new float2(1, 0),
                new float2(0, 1)
            );

            return OverlapObbObb(obb, aabbAsObb);
        }

        internal static bool OverlapObbObb(
            OBB a, OBB b
        )
        {
            return OverlapOnAxis(a, b, a.axisX)
                   && OverlapOnAxis(a, b, a.axisY)
                   && OverlapOnAxis(a, b, b.axisX)
                   && OverlapOnAxis(a, b, b.axisY);
        }
        
        public static float2 ClosestObbPoint(OBB obb, float2 point)
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
        
        public static void GetObbCorners(in OBB obb, out float2 c0, out float2 c1, out float2 c2, out float2 c3)
        {
            float2 ex = obb.axisX * obb.halfSize.x;
            float2 ey = obb.axisY * obb.halfSize.y;

            c0 = obb.center + ex + ey;
            c1 = obb.center - ex + ey;
            c2 = obb.center - ex - ey;
            c3 = obb.center + ex - ey;
        }
        
        static bool OverlapOnAxis(in OBB a, in OBB b, float2 axis)
        {
            ProjectObb(a, axis, out float minA, out float maxA);
            ProjectObb(b, axis, out float minB, out float maxB);

            return !(minA > maxB || minB > maxA);
        }

        static void ProjectObb(in OBB obb, float2 axis, out float min, out float max)
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