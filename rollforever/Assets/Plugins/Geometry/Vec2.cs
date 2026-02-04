using UnityEngine;

namespace Geometry
{
    public struct Vec2
    {
        public float x, y;

        public Vec2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public static Vec2 operator +(Vec2 a, Vec2 b) => new Vec2(a.x + b.x, a.y + b.y);

        public static Vec2 operator -(Vec2 a, Vec2 b) => new Vec2(a.x - b.x, a.y - b.y);

        public static Vec2 operator *(Vec2 a, float b) => new Vec2(a.x * b, a.y * b);
        
        public static Vec2 operator /(Vec2 a, float b) => new Vec2(a.x / b, a.y / b);

        public float Dot(Vec2 b) => x * b.x + y * b.y;

        public float Cross(Vec2 b) => x * b.y - y * b.x;

        public float SqrMagnitude => x * x + y * y;
        
        public float Magnitude => Mathf.Sqrt(SqrMagnitude);

        public Vec2 Normalized()
        {
            float m = Magnitude;
            float f = 1f / m;
            return m > Epsilon.Value ? new Vec2(x * f, y * f) : default;
        }
    }
}