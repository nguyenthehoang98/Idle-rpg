using Unity.Mathematics;

namespace Geometry.Primary
{
    public readonly struct Box
    {
        public readonly float2 min;
        public readonly float2 max;
        public readonly float2 center;
        public readonly float2 size;
        public readonly float2 halfsize;

        public Box(float2 min, float2 max)
        {
            this.min = min;
            this.max = max;
            center = (min + max) * 0.5f;
            size = max - min;
            halfsize = size / 2f;
        }

        private Box(float2 min, float2 max, float2 center, float2 size)
        {
            this.min = min;
            this.max = max;
            this.size = size;
            this.center = center;
            halfsize = size / 2f;
        }

        public static Box FromCenter(float2 center, float2 size)
        {
            float2 half = size * 0.5f;
            return new Box(center - half, center + half, center, size);
        }
    }
}