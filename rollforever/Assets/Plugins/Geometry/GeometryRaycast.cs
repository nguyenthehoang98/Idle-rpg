using Unity.Mathematics;

namespace Geometry
{
    public static class GeometryRaycast
    {
        public static bool Raycast(Ray ray, Circle c, out RayHit2D hit)
        {
            hit = default;

            float2 oc = ray.origin - c.center;
            float b = math.dot(oc, ray.dir);
            float cVal = math.dot(oc, oc) - c.radius * c.radius;
            float h = b * b - cVal;

            if (h < 0)
                return false;

            h = math.sqrt(h);
            float t = -b - h;

            if (t < 0)
                return false;

            hit.hit = true;
            hit.t = t;
            hit.point = ray.origin + ray.dir * t;
            hit.normal = math.normalize(hit.point - c.center);
            return true;
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

        private static bool Slab(float ro, float rd, float min, float max, ref float tMin, ref float tMax)
        {
            if (math.abs(rd) < float.Epsilon) return ro >= min && ro <= max;

            float ood = 1f / rd;
            float t1 = (min - ro) * ood;
            float t2 = (max - ro) * ood;

            if (t1 > t2) (t1, t2) = (t2, t1);

            tMin = math.max(tMin, t1);
            tMax = math.min(tMax, t2);

            return tMin <= tMax;
        }
    }
}