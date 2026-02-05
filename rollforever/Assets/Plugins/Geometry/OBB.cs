using Unity.Mathematics;

namespace Geometry
{
    public readonly struct OBB
    {
        public readonly float2 center;
        public readonly float2 halfSize;

        // local axes (đã normalize)
        public readonly float2 axisX;
        public readonly float2 axisY;

        public OBB(float2 center, float2 halfSize, float rotationRad)
        {
            this.center = center;
            this.halfSize = halfSize;

            float c = math.cos(rotationRad);
            float s = math.sin(rotationRad);

            axisX = new float2(c, s);
            axisY = new float2(-s, c);
        }

        public OBB(float2 center, float2 halfSize, float2 axisX, float2 axisY)
        {
            this.center = center;
            this.halfSize = halfSize;
            this.axisX = math.normalize(axisX);
            this.axisY = math.normalize(axisY);
        }
    }
}