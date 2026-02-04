namespace Geometry
{
    public struct Circle
    {
        public Vec2 center;
        public float radius;

        public Circle(Vec2 center, float radius)
        {
            this.center = center;
            this.radius = radius;
        }

        public float RadiusSqr => radius * radius;
    }
}