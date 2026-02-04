namespace Geometry
{
    public struct AABB
    {
        public Vec2 min;
        public Vec2 max;

        public AABB(Vec2 min, Vec2 max)
        {
            this.min = min;
            this.max = max;
        }

        public Vec2 Center => (min + max) * 0.5f;
        public Vec2 Size => max - min;
        public Vec2 Extents => Size * 0.5f;

        public float Width => max.x - min.x;
        public float Height => max.y - min.y;
    }
}