namespace Geometry
{
    /// <summary>
    /// Segment là hữu hạn
    /// </summary>
    public struct Segment
    {
        public Vec2 a;
        public Vec2 b;

        public Segment(Vec2 a, Vec2 b)
        {
            this.a = a;
            this.b = b;
        }
    }
}