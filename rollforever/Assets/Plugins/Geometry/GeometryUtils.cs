namespace Geometry
{
    public static class GeometryUtils
    {
        public static bool Overlaps(Shape shape, Shape other, out RayHit2D hit2D)
        {
            hit2D = new RayHit2D();
            if (shape.Type == ShapeType.Box && other.Type == ShapeType.Box)
            {
                hit2D.hit = GeometryAABB.Overlaps(
                    AABB.FromCenter(shape.CurrentPosition, shape.Size),
                    AABB.FromCenter(other.CurrentPosition, other.Size)
                );
            }
            else if (shape.Type == ShapeType.Circle && other.Type == ShapeType.Circle)
            {
                hit2D.hit = GeometryCircle.Overlaps(
                    new Circle(shape.CurrentPosition, shape.Radius),
                    new Circle(other.CurrentPosition, other.Radius)
                );
            }
            else if (shape.Type == ShapeType.Box && other.Type == ShapeType.Circle)
            {
                hit2D.hit = GeometryAABB.Overlaps(
                    AABB.FromCenter(shape.CurrentPosition, shape.Size),
                    new Circle(other.CurrentPosition, other.Radius)
                );
            }
            else if (shape.Type == ShapeType.Circle && other.Type == ShapeType.Box)
            {
                hit2D.hit = GeometryCircle.Overlaps(
                    new Circle(shape.CurrentPosition, shape.Radius),
                    AABB.FromCenter(other.CurrentPosition, other.Size)
                );
            }

            return false;
        }
    }

    public enum ShapeType
    {
        Circle, Box, Polygon,
    }
}