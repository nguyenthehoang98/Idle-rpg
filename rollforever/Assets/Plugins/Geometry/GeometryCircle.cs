using UnityEngine;

namespace Geometry
{
    public static class GeometryCircle
    {
        public static bool Contains(Circle c, Vec2 p)
        {
            return (p - c.center).SqrMagnitude <= c.RadiusSqr;
        }
        
        public static Vec2 ClosestPoint(Circle c, Vec2 p)
        {
            Vec2 d = p - c.center;
            float len = d.Magnitude;

            if (len < Epsilon.Value)
                return c.center + new Vec2(c.radius, 0);

            return c.center + d * (c.radius / len);
        }
        
        public static bool Intersect(Circle a, Circle b)
        {
            float r = a.radius + b.radius;
            return (a.center - b.center).SqrMagnitude <= r * r;
        }
        
        public static bool Intersect(Circle c, AABB box)
        {
            Vec2 closest = GeometryAABB.ClosestPoint(box, c.center);
            return (closest - c.center).SqrMagnitude <= c.RadiusSqr;
        }
        
        public static bool Intersect(Circle c, Segment seg)
        {
            Vec2 closest;
            GeometrySegment.PointSegment(c.center, seg, out closest);
            return (closest - c.center).SqrMagnitude <= c.RadiusSqr;
        }
        
        public static void DrawGizmos(Circle c, Color color, float dt, int segments = 24)
        {
            float step = Mathf.PI * 2f / segments;
            Vector3 prev = Vector3.zero;

            for (int i = 0; i <= segments; i++)
            {
                float a = step * i;
                Vector3 p = new Vector3(
                    c.center.x + Mathf.Cos(a) * c.radius,
                    c.center.y + Mathf.Sin(a) * c.radius
                );

                if (i > 0) Debug.DrawLine(prev, p, color, dt);

                prev = p;
            }
        }
    }
}