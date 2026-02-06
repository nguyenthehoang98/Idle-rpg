using Unity.Mathematics;

namespace Geometry
{
    /// <summary>
    /// Ray là vô hạn một phía
    /// </summary>
    public readonly struct Ray
    {
        public readonly float2 origin;
        public readonly float2 dir;

        public Ray(float2 origin, float2 dir)
        {
            this.origin = origin;
            float lenSq = math.lengthsq(dir);
            this.dir = lenSq > 1e-6f ? dir * math.rsqrt(lenSq) : float2.zero;
        }
    }

    public struct RayHit2D
    {
        public bool hit;
        public float length; // length along ray
        public float2 point; // hit position
        public float2 normal; // surface normal
    }
}