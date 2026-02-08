using Geometry.Math;
using Geometry.Primary;
using Unity.Mathematics;

namespace Geometry
{
    public static class GeometryUtils
    {
        public static bool Overlaps(
            Shape source, float2 sourcePrevPos, float2 sourceCurrPos,
            Shape target, float2 targetCurrPos)
        {
            if (source.type == ShapeType.Box && target.type == ShapeType.Box)
            {
                if (GeometryAABB.Overlaps(
                    AABB.FromCenter(sourcePrevPos, source.size),
                    AABB.FromCenter(targetCurrPos, target.size)
                )) return true;

                if (GeometryAABB.Overlaps(
                    AABB.FromCenter(sourceCurrPos, source.size),
                    AABB.FromCenter(targetCurrPos, target.size)
                )) return true;
            }
            else if (source.type == ShapeType.Circle && target.type == ShapeType.Circle)
            {
                if (GeometryCircle.Overlaps(
                    new Circle(sourcePrevPos, source.radius),
                    new Circle(targetCurrPos, target.radius)
                )) return true;

                if (GeometryCircle.Overlaps(
                    new Circle(sourceCurrPos, source.radius),
                    new Circle(targetCurrPos, target.radius)
                )) return true;
            }
            else if (source.type == ShapeType.Box && target.type == ShapeType.Circle)
            {
                if (GeometryAABB.Overlaps(
                    AABB.FromCenter(sourcePrevPos, source.size),
                    new Circle(targetCurrPos, target.radius)
                )) return true;

                if (GeometryAABB.Overlaps(
                    AABB.FromCenter(sourceCurrPos, source.size),
                    new Circle(targetCurrPos, target.radius)
                )) return true;
            }
            else if (source.type == ShapeType.Circle && target.type == ShapeType.Box)
            {
                if (GeometryCircle.Overlaps(
                    new Circle(sourcePrevPos, source.radius),
                    AABB.FromCenter(targetCurrPos, target.size)
                )) return true;

                if (GeometryCircle.Overlaps(
                    new Circle(sourceCurrPos, source.radius),
                    AABB.FromCenter(targetCurrPos, target.size)
                )) return true;
            }

            return false;
        }

        public static bool Sweep(
            Shape source, float2 sourcePrevPos, float2 sourceCurrPos,
            Shape target, float2 targetCurrPos)
        {
            if (source.type == ShapeType.Box && target.type == ShapeType.Box)
            {
                /*GeometrySweep.SweepAABBAABB(
                    sourcePrevPos, sourceCurrPos, source.Size * 0.5f,
                    AABB.FromCenter(targetCurrPos, target.Size), out hit2D
                );*/
            }
            else if (source.type == ShapeType.Circle && target.type == ShapeType.Circle)
            {
                return GeometryMath.SweepCircleCircle(
                    sourcePrevPos, sourceCurrPos, source.radius,
                    targetCurrPos, target.radius
                );
            }
            else if (source.type == ShapeType.Box && target.type == ShapeType.Circle)
            {
                /*GeometrySweep.SweepAABBCircle(
                    sourcePrevPos, sourceCurrPos, source.Size * 0.5f,
                    new Circle(targetCurrPos, target.radius), out hit2D 
                );*/
            }
            else if (source.type == ShapeType.Circle && target.type == ShapeType.Box)
            {
                /*GeometrySweep.SweepCircleAABB(
                    sourcePrevPos, sourceCurrPos, source.radius,
                    AABB.FromCenter(targetCurrPos, target.Size), out hit2D
                );*/
            }

            return false;
        }
    }
}