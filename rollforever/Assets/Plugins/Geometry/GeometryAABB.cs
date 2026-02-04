using System;
using System.Collections.Generic;
using UnityEngine;

namespace Geometry
{
    public class GeometryAABB
    {
        public static AABB Build(Vec2 center, Vec2 size)
        {
            Vec2 half = size * 0.5f;
            return new AABB(center - half, center + half);
        }
        
        public static AABB Build(IList<Vec2> points)
        {
            Vec2 min = points[0];
            Vec2 max = points[0];

            for (int i = 1; i < points.Count; i++)
            {
                Vec2 p = points[i];
                min.x = Mathf.Min(min.x, p.x);
                min.y = Mathf.Min(min.y, p.y);
                max.x = Mathf.Max(max.x, p.x);
                max.y = Mathf.Max(max.y, p.y);
            }

            return new AABB(min, max);
        }
        
        public static bool Contains(AABB aabb, Vec2 p)
        {
            return p.x >= aabb.min.x && p.x <= aabb.max.x && p.y >= aabb.min.y && p.y <= aabb.max.y;
        }
        
        public static Vec2 ComputeAABBNormal(Vec2 hitPoint, AABB box)
        {
            float left = Mathf.Abs(hitPoint.x - box.min.x);
            float right = Mathf.Abs(hitPoint.x - box.max.x);
            float bottom = Mathf.Abs(hitPoint.y - box.min.y);
            float top = Mathf.Abs(hitPoint.y - box.max.y);

            float min = Mathf.Min(left, right, bottom, top);

            if (Math.Abs(min - left) <= 0) return new Vec2(-1, 0);
            if (Math.Abs(min - right) <= 0) return new Vec2(1, 0);
            if (Math.Abs(min - bottom) <= 0) return new Vec2(0, -1);
            return new Vec2(0, 1);
        }
        
        public static bool Overlaps(AABB a, AABB b)
        {
            return a.min.x <= b.max.x && a.max.x >= b.min.x && a.min.y <= b.max.y && a.max.y >= b.min.y;
        }
        
        public static Vec2 ClosestPoint(AABB aabb, Vec2 p)
        {
            return new Vec2(
                Mathf.Clamp(p.x, aabb.min.x, aabb.max.x),
                Mathf.Clamp(p.y, aabb.min.y, aabb.max.y)
            );
        }
        
        public static bool Intersect(AABB box, Circle c)
        {
            float x = Mathf.Max(box.min.x, Mathf.Min(c.center.x, box.max.x));
            float y = Mathf.Max(box.min.y, Mathf.Min(c.center.y, box.max.y));

            float dx = x - c.center.x;
            float dy = y - c.center.y;

            return dx * dx + dy * dy <= c.radius * c.radius;
        }
        
        public static bool Intersect(AABB box, Segment seg)
        {
            // Liang–Barsky (2D)
            Vec2 d = seg.b - seg.a;
            float tMin = 0f;
            float tMax = 1f;

            if (!Clip(-d.x, seg.a.x - box.min.x, ref tMin, ref tMax)) return false;
            if (!Clip( d.x, box.max.x - seg.a.x, ref tMin, ref tMax)) return false;
            if (!Clip(-d.y, seg.a.y - box.min.y, ref tMin, ref tMax)) return false;
            if (!Clip( d.y, box.max.y - seg.a.y, ref tMin, ref tMax)) return false;

            return true;
        }

        static bool Clip(float p, float q, ref float tMin, ref float tMax)
        {
            if (Mathf.Abs(p) < Epsilon.Value)
                return q >= 0;

            float t = q / p;
            if (p < 0)
            {
                if (t > tMax) return false;
                if (t > tMin) tMin = t;
            }
            else
            {
                if (t < tMin) return false;
                if (t < tMax) tMax = t;
            }
            return true;
        }
        
        public static void DrawGizmos(AABB aabb, Color c, float dt)
        {
            Vector3 a = new Vector3(aabb.min.x, aabb.min.y);
            Vector3 b = new Vector3(aabb.max.x, aabb.min.y);
            Vector3 c1 = new Vector3(aabb.max.x, aabb.max.y);
            Vector3 d = new Vector3(aabb.min.x, aabb.max.y);

            Debug.DrawLine(a, b, c, dt);
            Debug.DrawLine(b, c1, c, dt);
            Debug.DrawLine(c1, d, c, dt);
            Debug.DrawLine(d, a, c, dt);
        }
    }
}