using Geometry;
using UnityEngine;
using Ray = Geometry.Ray;

public static class GeometryRaycast
{
    public static bool Raycast(Ray ray, Circle c, out RayHit2D hit)
    {
        hit = default;

        Vec2 oc = ray.origin - c.center;
        float b = oc.Dot(ray.dir);
        float cVal = oc.Dot(oc) - c.radius * c.radius;
        float h = b * b - cVal;

        if (h < 0)
            return false;

        h = Mathf.Sqrt(h);
        float t = -b - h;

        if (t < 0)
            return false;

        hit.hit = true;
        hit.t = t;
        hit.point = ray.origin + ray.dir * t;
        hit.normal = (hit.point - c.center).Normalized();
        return true;
    }

    public static bool Raycast(Ray ray, Segment seg, out RayHit2D hit)
    {
        hit = default;

        Vec2 r = ray.dir;
        Vec2 s = seg.b - seg.a;

        float rxs = r.Cross(s);
        if (Mathf.Abs(rxs) < Epsilon.Value)
            return false;

        Vec2 qp = seg.a - ray.origin;

        float t = qp.Cross(s) / rxs;
        float u = qp.Cross(r) / rxs;

        if (t >= 0 && u >= 0 && u <= 1)
        {
            hit.hit = true;
            hit.t = t;
            hit.point = ray.origin + r * t;

            // normal = perpendicular của segment
            Vec2 edge = (seg.b - seg.a).Normalized();
            hit.normal = new Vec2(-edge.y, edge.x);

            return true;
        }

        return false;
    }

    public static bool Raycast(Ray ray, AABB box, out RayHit2D hit)
    {
        hit = default;

        float tMin = 0;
        float tMax = float.MaxValue;

        if (!Slab(ray.origin.x, ray.dir.x, box.min.x, box.max.x, ref tMin, ref tMax)) return false;
        if (!Slab(ray.origin.y, ray.dir.y, box.min.y, box.max.y, ref tMin, ref tMax)) return false;

        hit.hit = true;
        hit.t = tMin;
        hit.point = ray.origin + ray.dir * tMin;
        hit.normal = GeometryAABB.ComputeAABBNormal(hit.point, box);
        return true;
    }

    public static bool Raycast(Ray ray, Line line, out RayHit2D hit)
    {
        hit = default;

        float rxs = ray.dir.Cross(line.dir);
        if (Mathf.Abs(rxs) < Epsilon.Value)
            return false;

        Vec2 qp = line.point - ray.origin;
        float t = qp.Cross(line.dir) / rxs;

        if (t < 0)
            return false;

        hit.hit = true;
        hit.t = t;
        hit.point = ray.origin + ray.dir * t;
        hit.normal = new Vec2(-line.dir.y, line.dir.x);
        return true;
    }
    
    private static bool Slab(float ro, float rd, float min, float max, ref float tMin, ref float tMax)
    {
        if (Mathf.Abs(rd) < Epsilon.Value)
            return ro >= min && ro <= max;

        float ood = 1f / rd;
        float t1 = (min - ro) * ood;
        float t2 = (max - ro) * ood;

        if (t1 > t2) (t1, t2) = (t2, t1);

        tMin = Mathf.Max(tMin, t1);
        tMax = Mathf.Min(tMax, t2);

        return tMin <= tMax;
    }

    public static void DrawGizmos(Ray r, Color c, float dt)
    {
        Vector3 o = new Vector3(r.origin.x, r.origin.y);
        Debug.DrawLine(o, o + new Vector3(r.dir.x, r.dir.y) * 10f, c, dt);
    }

    public static void DrawGizmos(RayHit2D hit, Color c, float dt)
    {
        GeometryCircle.DrawGizmos(
            new Circle(hit.point, 0.1f), c, dt
        );
        Debug.DrawLine(
            new Vector3(hit.point.x, hit.point.y),
            new Vector3(hit.point.x + hit.normal.x * 0.5f, hit.point.y + hit.normal.y * 0.5f),
            c, dt
        );
    }
}