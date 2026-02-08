using Geometry.Primary;
using Unity.Mathematics;

namespace Geometry.Math
{
    internal static partial class GeometryMath
    {
        internal static bool IsBoxContainPoint(
            Box box, float2 point
        )
        {
            return point.x >= box.min.x &&
                   point.x <= box.max.x &&
                   point.y >= box.min.y &&
                   point.y <= box.max.y;
        }

        internal static bool OverlapBoxBox(Box a, Box b)
        {
            return a.min.x <= b.max.x &&
                   a.max.x >= b.min.x &&
                   a.min.y <= b.max.y &&
                   a.max.y >= b.min.y;
        }

        internal static bool OverlapBoxCircle(Box a, Circle b)
        {
            return OverlapCircleBox(b, a);
        }

        internal static float2 ClosestCirclePoint(
            Box box, float2 point
        )
        {
            return new float2(
                math.clamp(point.x, box.min.x, box.max.x),
                math.clamp(point.y, box.min.y, box.max.y)
            );
        }
    }
}