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
            const float EPS = 1e-6f;

            if (math.abs(rd) < EPS)
            {
                // Ray song song slab
                return ro >= min && ro <= max;
            }

            float ood = 1f / rd;
            float t1 = (min - ro) * ood;
            float t2 = (max - ro) * ood;

            if (t1 > t2)
                (t1, t2) = (t2, t1);

            tMin = math.max(tMin, t1);
            tMax = math.min(tMax, t2);

            return tMin <= tMax;
        }
        
        public static bool Raycast(Ray ray, AABB box, out float t, out float2 normal)
        {
            t = 0;
            normal = float2.zero;

            float2 invDir = 1.0f / ray.dir;

            float2 t1 = (box.min - ray.origin) * invDir;
            float2 t2 = (box.max - ray.origin) * invDir;

            float2 tmin = math.min(t1, t2);
            float2 tmax = math.max(t1, t2);

            float entry = math.max(tmin.x, tmin.y);
            float exit  = math.min(tmax.x, tmax.y);

            if (exit < 0 || entry > exit || entry > 1)
                return false;

            t = math.max(entry, 0);

            // normal
            if (math.abs(t - tmin.x) <= 0)
                normal = new float2(-math.sign(ray.dir.x), 0);
            else
                normal = new float2(0, -math.sign(ray.dir.y));

            return true;
        }
        
        public static bool Raycast(Ray ray, Circle circle, out float t, out float2 normal)
        {
            t = 0;
            normal = float2.zero;

            float2 m = ray.origin - circle.center;
            float b = math.dot(m, ray.dir);
            float c = math.dot(m, m) - circle.radius * circle.radius;

            // ray đang hướng ra xa
            if (c > 0 && b > 0)
                return false;

            float discr = b * b - c * math.dot(ray.dir, ray.dir);
            if (discr < 0)
                return false;

            t = (-b - math.sqrt(discr)) / math.dot(ray.dir, ray.dir);
            if (t < 0 || t > 1)
                return false;

            float2 hitPoint = ray.origin + ray.dir * t;
            normal = math.normalize(hitPoint - circle.center);
            return true;
        }

    }
}