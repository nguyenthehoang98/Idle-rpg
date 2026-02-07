using Geometry.Math;
using Geometry.Primary;
using Unity.Mathematics;

namespace Geometry
{
    public static class GeometryUtils
    {
        public static bool Overlaps(
            ShapeInstance source, float2 sourcePrevPos, float2 sourceCurrPos,
            ShapeInstance target, float2 targetCurrPos)
        {
            if (source.Type == ShapeType.Box && target.Type == ShapeType.Box)
            {
                if (GeometryAABB.Overlaps(
                    AABB.FromCenter(sourcePrevPos, source.Size),
                    AABB.FromCenter(targetCurrPos, target.Size)
                )) return true;

                if (GeometryAABB.Overlaps(
                    AABB.FromCenter(sourceCurrPos, source.Size),
                    AABB.FromCenter(targetCurrPos, target.Size)
                )) return true;
            }
            else if (source.Type == ShapeType.Circle && target.Type == ShapeType.Circle)
            {
                if (GeometryCircle.Overlaps(
                    new Circle(sourcePrevPos, source.Radius),
                    new Circle(targetCurrPos, target.Radius)
                )) return true;

                if (GeometryCircle.Overlaps(
                    new Circle(sourceCurrPos, source.Radius),
                    new Circle(targetCurrPos, target.Radius)
                )) return true;
            }
            else if (source.Type == ShapeType.Box && target.Type == ShapeType.Circle)
            {
                if (GeometryAABB.Overlaps(
                    AABB.FromCenter(sourcePrevPos, source.Size),
                    new Circle(targetCurrPos, target.Radius)
                )) return true;

                if (GeometryAABB.Overlaps(
                    AABB.FromCenter(sourceCurrPos, source.Size),
                    new Circle(targetCurrPos, target.Radius)
                )) return true;
            }
            else if (source.Type == ShapeType.Circle && target.Type == ShapeType.Box)
            {
                if (GeometryCircle.Overlaps(
                    new Circle(sourcePrevPos, source.Radius),
                    AABB.FromCenter(targetCurrPos, target.Size)
                )) return true;

                if (GeometryCircle.Overlaps(
                    new Circle(sourceCurrPos, source.Radius),
                    AABB.FromCenter(targetCurrPos, target.Size)
                )) return true;
            }

            return false;
        }

        public static bool Sweep(
            ShapeInstance source, float2 sourcePrevPos, float2 sourceCurrPos,
            ShapeInstance target, float2 targetCurrPos)
        {
            if (source.Type == ShapeType.Box && target.Type == ShapeType.Box)
            {
                /*GeometrySweep.SweepAABBAABB(
                    sourcePrevPos, sourceCurrPos, source.Size * 0.5f,
                    AABB.FromCenter(targetCurrPos, target.Size), out hit2D
                );*/
            }
            else if (source.Type == ShapeType.Circle && target.Type == ShapeType.Circle)
            {
                return GeometryMath.SweepCircleCircle(
                    sourcePrevPos, sourceCurrPos, source.Radius,
                    targetCurrPos, target.Radius
                );
            }
            else if (source.Type == ShapeType.Box && target.Type == ShapeType.Circle)
            {
                /*GeometrySweep.SweepAABBCircle(
                    sourcePrevPos, sourceCurrPos, source.Size * 0.5f,
                    new Circle(targetCurrPos, target.Radius), out hit2D 
                );*/
            }
            else if (source.Type == ShapeType.Circle && target.Type == ShapeType.Box)
            {
                /*GeometrySweep.SweepCircleAABB(
                    sourcePrevPos, sourceCurrPos, source.Radius,
                    AABB.FromCenter(targetCurrPos, target.Size), out hit2D
                );*/
            }

            return false;
        }
    }

    public enum ShapeType
    {
        Circle, Box,
    }
}