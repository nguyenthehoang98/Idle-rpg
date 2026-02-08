using Unity.Mathematics;

namespace Geometry.Math
{
    // ray calculator
    // Mục tiêu bắn ray tới các shape và kiểm tra giao nhau
    // Thực chất là giải phương trình bậc 2.
    internal static partial class GeometryMath
    {
        /// <summary>
        /// Ray -> Circle (với length)
        /// </summary>
        /// <param name="rayPoint">Điểm gốc của Ray</param>
        /// <param name="rayDir">Hướng của Ray</param>
        /// <param name="rayLength">Độ dài của Ray</param>
        /// <param name="circleCenter">Tâm hình tròn</param>
        /// <param name="circleRadius">Bán kinh hình tròn</param>
        /// <returns></returns>
        internal static bool RayCircle(
            float2 rayPoint, float2 rayDir, float rayLength,
            float2 circleCenter, float circleRadius)
        {
            float dirLenSq = math.lengthsq(rayDir);
            if (dirLenSq < math.EPSILON)
                return false;

            float invLen = math.rsqrt(dirLenSq);
            float2 d = rayDir * invLen; // normalize
            float2 m = rayPoint - circleCenter;

            float b = math.dot(m, d);
            float c = math.dot(m, m) - circleRadius * circleRadius;

            // ray starts outside & pointing away
            if (c > 0f && b > 0f)
                return false;

            float discr = b * b - c;
            if (discr < 0f)
                return false;

            float t = -b - math.sqrt(discr);

            // ray segment check
            return t >= 0f && t <= rayLength;
        }
    }
}