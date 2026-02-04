using UnityEngine;

namespace Geometry
{
    public class GeometrySegment
    {
        public static float PointSegment(Vec2 p, Segment s, out Vec2 closest)
        {
            Vec2 ab = s.b - s.a;
            float t = (p - s.a).Dot(ab) / ab.Dot(ab);
            t = Mathf.Clamp01(t);

            closest = s.a + ab * t;
            return (p - closest).SqrMagnitude;
        }
        
        public static void DrawGizmos(Segment s, Color c, float dt)
        {
            Debug.DrawLine(new Vector3(s.a.x, s.a.y), new Vector3(s.b.x, s.b.y), c, dt);
        }
    }
}