using System;
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
                if (GeometryMath.OverlapBoxBox(
                    Box.FromCenter(sourcePrevPos, source.size),
                    Box.FromCenter(targetCurrPos, target.size)
                )) return true;

                if (GeometryMath.OverlapBoxBox(
                    Box.FromCenter(sourceCurrPos, source.size),
                    Box.FromCenter(targetCurrPos, target.size)
                )) return true;
            }
            else if (source.type == ShapeType.Circle && target.type == ShapeType.Circle)
            {
                if (GeometryMath.OverlapsCircleCircle(
                    new Circle(sourcePrevPos, source.radius),
                    new Circle(targetCurrPos, target.radius)
                )) return true;

                if (GeometryMath.OverlapsCircleCircle(
                    new Circle(sourceCurrPos, source.radius),
                    new Circle(targetCurrPos, target.radius)
                )) return true;
            }
            else if (source.type == ShapeType.Box && target.type == ShapeType.Circle)
            {
                if (GeometryMath.OverlapBoxCircle(
                    Box.FromCenter(sourcePrevPos, source.size),
                    new Circle(targetCurrPos, target.radius)
                )) return true;

                if (GeometryMath.OverlapBoxCircle(
                    Box.FromCenter(sourceCurrPos, source.size),
                    new Circle(targetCurrPos, target.radius)
                )) return true;
            }
            else if (source.type == ShapeType.Circle && target.type == ShapeType.Box)
            {
                if (GeometryMath.OverlapCircleBox(
                    new Circle(sourcePrevPos, source.radius),
                    Box.FromCenter(targetCurrPos, target.size)
                )) return true;

                if (GeometryMath.OverlapCircleBox(
                    new Circle(sourceCurrPos, source.radius),
                    Box.FromCenter(targetCurrPos, target.size)
                )) return true;
            }
            else
            {
#if DEVELOP_MODE
                throw new Exception($"Overlaps [{source.type}, {target.type}] chưa được xác định");     
#endif
            }
            return false;
        }

        public static bool Sweep(
            Shape source, float2 sourcePrevPos, float2 sourceCurrPos,
            Shape target, float2 targetCurrPos)
        {
            if (source.type == ShapeType.Box && target.type == ShapeType.Box)
            {
                return GeometryMath.SweepBoxBox(
                    sourcePrevPos,
                    Box.FromCenter(sourceCurrPos, source.size),
                    Box.FromCenter(targetCurrPos, target.size)
                );
            }
            else if (source.type == ShapeType.Circle && target.type == ShapeType.Circle)
            {
                return GeometryMath.SweepCircleCircle(
                    sourcePrevPos,
                    new Circle(sourceCurrPos, source.radius),
                    new Circle(targetCurrPos, target.radius)
                );
            }
            else if (source.type == ShapeType.Box && target.type == ShapeType.Circle)
            {
                return GeometryMath.SweepBoxCircle(
                    sourcePrevPos,
                    Box.FromCenter(sourceCurrPos, source.size),
                    new Circle(targetCurrPos, target.radius)
                );
            }
            else if (source.type == ShapeType.Circle && target.type == ShapeType.Box)
            {
                return GeometryMath.SweepCircleBox(
                    sourcePrevPos,
                    new Circle(sourceCurrPos, source.radius),
                    Box.FromCenter(targetCurrPos, target.size)
                );
            }
            else
            {
#if DEVELOP_MODE
                throw new Exception($"Sweep [{source.type}, {target.type}] chưa được xác định");     
#endif
            }

            return false;
        }
    }
}