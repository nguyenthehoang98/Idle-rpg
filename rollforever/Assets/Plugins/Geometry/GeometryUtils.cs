using System;
using Geometry.Math;
using Geometry.Primary;
using Unity.Mathematics;

namespace Geometry
{
    public static class GeometryUtils
    {
        public static bool Ray(
            Ray ray, float length, Shape shape, float2 shapeCurrPos, 
            out float hitLength, out float2 hitPoint
        )
        {
            if (shape.type == ShapeType.Circle)
            {
                return GeometryMath.RayCircle(ray, new Circle(shapeCurrPos, shape.radius), length,
                    out hitLength, out hitPoint);
            }
            else if (shape.type == ShapeType.Box)
            {
                return GeometryMath.RayBox(ray, Box.FromCenter(shapeCurrPos, shape.size), length,
                    out hitLength, out hitPoint);
            }
            else
            {
                throw new NotImplementedException("Unknown shape type");
            }
        }
        
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
        
        public static void CalculateBounds(float2[] points, float2 cellSize, out float2 boxSize)
        {
            if (points == null || points.Length == 0)
            {
                boxSize = float2.zero;
                return;
            }

            float2 halfCell = cellSize * 0.5f;

            float minX = points[0].x;
            float maxX = points[0].x;
            float minY = points[0].y;
            float maxY = points[0].y;

            for (int i = 1; i < points.Length; i++)
            {
                float2 p = points[i];

                if (p.x < minX) minX = p.x;
                if (p.x > maxX) maxX = p.x;
                if (p.y < minY) minY = p.y;
                if (p.y > maxY) maxY = p.y;
            }

            // Expand theo kích thước cell
            minX -= halfCell.x;
            maxX += halfCell.x;
            minY -= halfCell.y;
            maxY += halfCell.y;

            boxSize = new float2(maxX - minX, maxY - minY);
        }
    }
}