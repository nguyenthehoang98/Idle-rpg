using Unity.Mathematics;

namespace Geometry
{
    public readonly struct Polygon
    {
        public readonly int count;
        readonly float2 pivot;
        readonly float2[] points;
        readonly float2 axisX;
        readonly float2 axisY;

        public Polygon(float2 center, float2[] localPoints, float rotationRad)
        {
            pivot = center;
            points = localPoints;

            float c = math.cos(rotationRad);
            float s = math.sin(rotationRad);

            axisX = new float2(c, s);
            axisY = new float2(-s, c);

            count = localPoints.Length;
        }

        public float2 GetWorldPoint(int i)
        {
            float2 p = points[i];
            return pivot + axisX * p.x + axisY * p.y;
        }
    }
}