using Geometry.Primary;
using Unity.Mathematics;
using UnityEngine;

namespace Geometry.Math
{
    // circle calculator
    internal static partial class GeometryMath
    {
        internal static bool IsPointOnCircumference(
            Circle circle, float2 point
        )
        {
            float2 d = point - circle.center;
            float distSq = math.dot(d, d);
            float rSq = circle.radius * circle.radius;
            return math.abs(distSq - rSq) <= EPSILON;
        }

        internal static bool IsCircleContainPoint(
            Circle circle, float2 point
        )
        {
            float distSq = math.lengthsq(point - circle.center);
            float rSq = circle.radius * circle.radius;
            return distSq <= rSq;
        }

        internal static float2 ClosestCirclePoint(
            Circle circle, float2 point
        )
        {
            float2 d = point - circle.center;
            float lenSq = math.lengthsq(d);
            float r = circle.radius;

            if (lenSq <= EPSILON)
            {
                return circle.center + new float2(r, 0f);
            }

            float invLen = math.rsqrt(lenSq);
            return circle.center + d * (r * invLen);
        }

        internal static bool OverlapsCircleCircle(
            Circle a, Circle b
        )
        {
            float r = a.radius + b.radius;
            return math.lengthsq(a.center - b.center) <= r * r + EPSILON;
        }

        internal static bool OverlapCircleBox(
            Circle a, Box b
        )
        {
            float x = math.max(b.min.x, math.min(a.center.x, b.max.x));
            float y = math.max(b.min.y, math.min(a.center.y, b.max.y));
            float dx = x - a.center.x;
            float dy = y - a.center.y;
            return dx * dx + dy * dy <= a.radius * a.radius;
        }
    }
}