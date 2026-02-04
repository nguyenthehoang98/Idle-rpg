namespace Geometry
{
    /// <summary>
    /// Line là vô hạn hai phía
    /// </summary>
    public struct Line
    {
        public Vec2 point;
        public Vec2 dir; // normalized

        public Line(Vec2 point, Vec2 dir)
        {
            this.point = point;
            this.dir = dir.Normalized();
        }
    }
}