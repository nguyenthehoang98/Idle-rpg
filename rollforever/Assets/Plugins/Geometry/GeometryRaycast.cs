using Unity.Mathematics;

namespace Geometry
{
    public static class GeometryRaycast
    {
        public static void Raycast(Ray ray, float length, Circle c, out RayHit2D hit)
        {
            hit = default;

            float2 oc = ray.origin - c.center;

            float b = math.dot(oc, ray.dir);
            float cVal = math.dot(oc, oc) - c.radius * c.radius;

            float h = b * b - cVal;
            if (h < 0f)
                return;

            h = math.sqrt(h);

            // nghiệm gần nhất
            float t = -b - h;

            // nếu nghiệm này nằm sau origin hoặc vượt quá length thì reject
            if (t < 0f || t > length)
                return;

            hit.hit = true;
            hit.length = t;
            hit.point = ray.origin + ray.dir * t;
            hit.normal = math.normalize(hit.point - c.center);
        }

        public static void Raycast(Ray ray, float length, AABB box, out RayHit2D hit)
        {
            hit = default;

            float tMin = 0f;
            float tMax = length;

            if (!Slab(ray.origin.x, ray.dir.x, box.min.x, box.max.x, ref tMin, ref tMax)) return;
            if (!Slab(ray.origin.y, ray.dir.y, box.min.y, box.max.y, ref tMin, ref tMax)) return;

            if (tMin < 0f)
                return;

            hit.hit = true;
            hit.length = tMin;
            hit.point = ray.origin + ray.dir * tMin;
            hit.normal = GeometryAABB.ComputeAABBNormal(hit.point, box);
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