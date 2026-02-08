using Geometry.Primary;
using Unity.Mathematics;

namespace Geometry.Math
{
    // ray calculator
    // Mục tiêu bắn ray tới các shape và kiểm tra giao nhau
    // Thực chất là giải phương trình bậc 2.
    internal static partial class GeometryMath
    {
        // Ray -> Circle (với length)
        internal static bool RayCircle(
            Ray ray, Circle circle, float length
        )
        {
            float dirLenSq = math.lengthsq(ray.dir);
            if (dirLenSq < math.EPSILON)
                return false;

            float invLen = math.rsqrt(dirLenSq);
            float2 d = ray.dir * invLen; // normalize
            float2 m = ray.origin - circle.center;

            float b = math.dot(m, d);
            float c = math.dot(m, m) - circle.radius * circle.radius;

            // ray starts outside & pointing away
            if (c > 0f && b > 0f)
                return false;

            float discr = b * b - c;
            if (discr < 0f)
                return false;

            float t = -b - math.sqrt(discr);

            // ray segment check
            return t >= 0f && t <= length;
        }
        
        internal static bool RayCircle(
            Ray ray, Circle circle, float length, out float hitLength, out float2 hitPoint
        )
        {
            hitLength = 0;
            hitPoint = float2.zero;
            
            float dirLenSq = math.lengthsq(ray.dir);
            if (dirLenSq < math.EPSILON)
                return false;

            float invLen = math.rsqrt(dirLenSq);
            float2 d = ray.dir * invLen; // normalize
            float2 m = ray.origin - circle.center;

            float b = math.dot(m, d);
            float c = math.dot(m, m) - circle.radius * circle.radius;

            // ray starts outside & pointing away
            if (c > 0f && b > 0f)
                return false;

            float discr = b * b - c;
            if (discr < 0f)
                return false;

            float t = -b - math.sqrt(discr);

            if (t < 0f || t > length)
                return false;

            hitPoint = ray.origin + ray.dir * t;
            hitLength = t;
            return true;
        }

        internal static bool RayBox(
            Ray ray, Box box, float length
        )
        {
            float tMin = 0f;
            float tMax = length;

            if (!Slab(ray.origin.x, ray.dir.x, box.min.x, box.max.x, ref tMin, ref tMax)) return false;
            if (!Slab(ray.origin.y, ray.dir.y, box.min.y, box.max.y, ref tMin, ref tMax)) return false;

            if (tMin < 0f)
                return false;

            return true;
        }

        internal static bool RayBox(
            Ray ray, Box box, float length, out float hitLength, out float2 hitPoint
        )
        {
            float tMin = 0f;
            float tMax = length;
            hitLength = 0;
            hitPoint = float2.zero;

            if (!Slab(ray.origin.x, ray.dir.x, box.min.x, box.max.x, ref tMin, ref tMax)) return false;
            if (!Slab(ray.origin.y, ray.dir.y, box.min.y, box.max.y, ref tMin, ref tMax)) return false;

            if (tMin < 0f)
                return false;

            hitLength = tMin;
            hitPoint = ray.origin + ray.dir * tMin;
            return true;
        }
        
        static bool Slab(float ro, float rd, float min, float max, ref float tMin, ref float tMax)
        {
            if (math.abs(rd) < EPSILON)
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
    }
}