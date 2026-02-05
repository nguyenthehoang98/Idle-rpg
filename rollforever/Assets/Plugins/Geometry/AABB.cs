using Unity.Mathematics;

namespace Geometry
{
    public readonly struct AABB
    {
        public readonly float2 min;
        public readonly float2 max;

        public AABB(float2 min, float2 max)
        {
            this.min = min;
            this.max = max;
        }

        public static AABB FromCenter(float2 center, float2 size)
        {
            float2 half = size * 0.5f;
            return new AABB(center - half, center + half);
        }
    }
}