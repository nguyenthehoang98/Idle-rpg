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
            this.dir = math.normalize(dir);
        }
    }
    
    public struct RayHit2D
    {
        public bool hit;
        public float t;        // distance along ray
        public float2 point;     // hit position
        public float2 normal;    // surface normal
    }
}