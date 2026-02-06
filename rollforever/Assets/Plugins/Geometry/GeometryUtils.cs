namespace Geometry
{
    public static class GeometryUtils
    {
        public static bool Overlaps(ShapeInstance source, ShapeInstance target, out RayHit2D hit2D)
        {
            hit2D = new RayHit2D();
            if (source.Type == ShapeType.Box && target.Type == ShapeType.Box)
            {
                hit2D.hit = GeometryAABB.Overlaps(
                    AABB.FromCenter(source.CurrentPosition, source.Size),
                    AABB.FromCenter(target.CurrentPosition, target.Size)
                );
            }
            else if (source.Type == ShapeType.Circle && target.Type == ShapeType.Circle)
            {
                hit2D.hit = GeometryCircle.Overlaps(
                    new Circle(source.CurrentPosition, source.Radius),
                    new Circle(target.CurrentPosition, target.Radius)
                );
            }
            else if (source.Type == ShapeType.Box && target.Type == ShapeType.Circle)
            {
                hit2D.hit = GeometryAABB.Overlaps(
                    AABB.FromCenter(source.CurrentPosition, source.Size),
                    new Circle(target.CurrentPosition, target.Radius)
                );
            }
            else if (source.Type == ShapeType.Circle && target.Type == ShapeType.Box)
            {
                hit2D.hit = GeometryCircle.Overlaps(
                    new Circle(source.CurrentPosition, source.Radius),
                    AABB.FromCenter(target.CurrentPosition, target.Size)
                );
            }

            return false;
        }

        public static void Sweep(ShapeInstance source, ShapeInstance target, out RayHit2D hit2D)
        {
            hit2D = new RayHit2D();
            if (source.Type == ShapeType.Box && target.Type == ShapeType.Box)
            {
               
            }
            else if (source.Type == ShapeType.Circle && target.Type == ShapeType.Circle)
            {
                GeometrySweep.SweepCircleCircle(
                    source.PrefPosition, source.CurrentPosition, source.Radius,
                    new Circle(target.CurrentPosition, target.Radius), out hit2D
                );
            }
            else if (source.Type == ShapeType.Box && target.Type == ShapeType.Circle)
            {
              
            }
            else if (source.Type == ShapeType.Circle && target.Type == ShapeType.Box)
            {
               
            }
        }
    }

    public enum ShapeType
    {
        Circle, Box,
    }
}