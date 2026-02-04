namespace Geometry
{
    /// <summary>
    /// Ray là vô hạn một phía
    /// </summary>
    public struct Ray
    {
        public Vec2 origin;
        public Vec2 dir; // normalized

        public Ray(Vec2 origin, Vec2 dir)
        {
            this.origin = origin;
            this.dir = dir;
        }
    }
    
    public struct RayHit2D
    {
        public bool hit;
        public float t;        // distance along ray
        public Vec2 point;     // hit position
        public Vec2 normal;    // surface normal
    }
}