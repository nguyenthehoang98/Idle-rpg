using Unity.Mathematics;

namespace Geometry.Math
{
    // sweep calculator
    // Xử lý kiểm tra va chạm 1 vật di chuyển với 1 vật đứng yên
    internal static partial class GeometryMath
    {
        internal static bool SweepCircleCircle(
            float2 aPrevPos, float2 aCurrPos, float aRadius,
            float2 bCurrPos, float bRadius
        )
        {
            float2 delta = aCurrPos - aPrevPos;
            float dist = math.length(delta);
            if (dist <= 0f)
                return false;

            return RayCircle(
                aPrevPos, delta / dist, dist,
                bCurrPos, aRadius + bRadius
            );
        }
    }
}