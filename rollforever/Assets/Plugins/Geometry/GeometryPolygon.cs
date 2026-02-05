using Unity.Mathematics;

namespace Geometry
{
    public static class GeometryPolygon
    {
        public static bool Overlaps(in Polygon poly, in Circle circle)
        {
            for (int i = 0; i < poly.count; i++)
                if (!OverlapOnAxis(poly, circle, GetAxis(poly, i)))
                    return false;

            float2 closest = poly.GetWorldPoint(0);
            float minDist = math.lengthsq(circle.center - closest);

            for (int i = 1; i < poly.count; i++)
            {
                float2 p = poly.GetWorldPoint(i);
                float d = math.lengthsq(circle.center - p);
                if (d < minDist)
                {
                    minDist = d;
                    closest = p;
                }
            }

            float2 axis = math.normalize(circle.center - closest);
            return OverlapOnAxis(poly, circle, axis);
        }

        public static bool Overlaps(in Polygon poly, in OBB obb)
        {
            GeometryOBB.GetCorners(obb, out var c0, out var c1, out var c2, out var c3);
            Polygon p = new Polygon(float2.zero, new[] {c0, c1, c2, c3}, 0);
            return Overlaps(poly, p);
        }

        static bool OverlapOnAxis(in Polygon poly, in Circle c, float2 axis)
        {
            ProjectPolygon(poly, axis, out float minA, out float maxA);

            float center = math.dot(c.center, axis);
            float minB = center - c.radius;
            float maxB = center + c.radius;

            return !(minA > maxB || minB > maxA);
        }

        public static bool Overlaps(in Polygon a, in Polygon b)
        {
            for (int i = 0; i < a.count; i++)
                if (!OverlapOnAxis(a, b, GetAxis(a, i)))
                    return false;

            for (int i = 0; i < b.count; i++)
                if (!OverlapOnAxis(a, b, GetAxis(b, i)))
                    return false;

            return true;
        }

        static bool OverlapOnAxis(in Polygon a, in Polygon b, float2 axis)
        {
            ProjectPolygon(a, axis, out float minA, out float maxA);
            ProjectPolygon(b, axis, out float minB, out float maxB);

            return !(minA > maxB || minB > maxA);
        }

        static float2 GetAxis(in Polygon poly, int i)
        {
            int j = (i + 1) % poly.count;

            float2 p0 = poly.GetWorldPoint(i);
            float2 p1 = poly.GetWorldPoint(j);

            float2 edge = p1 - p0;
            return math.normalize(new float2(-edge.y, edge.x));
        }

        static void ProjectPolygon(in Polygon poly, float2 axis, out float min, out float max)
        {
            float d = math.dot(poly.GetWorldPoint(0), axis);
            min = max = d;

            for (int i = 1; i < poly.count; i++)
            {
                d = math.dot(poly.GetWorldPoint(i), axis);
                min = math.min(min, d);
                max = math.max(max, d);
            }
        }
    }
}