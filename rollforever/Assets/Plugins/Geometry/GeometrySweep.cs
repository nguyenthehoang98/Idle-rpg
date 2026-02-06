using Unity.Mathematics;
using UnityEngine;

namespace Geometry
{
    public static class GeometrySweep
    {
        public static void SweepCircleCircle(float2 prevCenter, float2 currCenter, float radiusA, Circle b, out RayHit2D hit)
        {
            hit = default;

            float2 delta = currCenter - prevCenter;
            float dist = math.length(delta);
            if (dist <= 0f)
                return;

            Ray ray = new Ray(prevCenter, delta / dist);

            Circle expanded = new Circle(
                b.center,
                radiusA + b.radius
            );

            GeometryRaycast.Raycast(ray, dist, expanded, out hit);
        }
        
        public static void SweepCircleAABB(float2 prevCenter, float2 currCenter, float radius, AABB box, out RayHit2D hit)
        {
            hit = default;

            float2 delta = currCenter - prevCenter;
            float dist = math.length(delta);

            if (dist <= float.Epsilon)
                return;

            float2 dir = delta / dist;

            AABB expanded = new AABB(
                box.min - radius,
                box.max + radius
            );

            bool contains = GeometryAABB.Contains(expanded, prevCenter);
            if (contains)
            {
                hit.hit = true;
                hit.length = 0f;
                hit.point = prevCenter;
                hit.normal = math.normalize(prevCenter - GeometryAABB.ClosestPoint(expanded, prevCenter));
                return;
            }

            Ray ray = new Ray(prevCenter, dir);
            GeometryRaycast.Raycast(ray, dist, expanded, out hit);
        }
        
        public static void SweepCircleOBB(float2 prevCenter, float2 currCenter, float radius, OBB obb, out RayHit2D hit)
        {
            hit = default;

            // Transform centers to OBB local space
            float2 localPrev = WorldToOBBLocal(prevCenter, obb);
            float2 localCurr = WorldToOBBLocal(currCenter, obb);

            float2 delta = localCurr - localPrev;
            float dist = math.length(delta);

            if (dist <= float.Epsilon)
                return;

            float2 dir = delta / dist;

            // Local AABB of OBB
            AABB localAABB = OBBToLocalAABB(obb);

            // Expand AABB by circle radius
            AABB expanded = new AABB(
                localAABB.min - radius,
                localAABB.max + radius
            );

            // Initial overlap
            if (GeometryAABB.Contains(expanded, localPrev))
            {
                hit.hit = true;
                hit.length = 0f;

                float2 cp = GeometryAABB.ClosestPoint(localAABB, localPrev);
                float2 localNormal = math.normalize(localPrev - cp);

                // Transform normal back to world
                hit.normal = DirFromOBBLocal(localNormal, obb);
                hit.point = prevCenter;
                return;
            }

            Ray ray = new Ray(localPrev, dir);

            GeometryRaycast.Raycast(ray, dist, expanded, out hit);

            if (!hit.hit)
                return;

            // Convert hit back to world space
            hit.point = OBBLocalToWorld(hit.point, obb);
            hit.normal = DirFromOBBLocal(hit.normal, obb);
        }

        public static void SweepAABBAABB(float2 prevCenter, float2 currCenter, float2 halfSize, AABB target, out RayHit2D hit)
        {
            hit = default;

            float2 delta = currCenter - prevCenter;
            float dist = math.length(delta);

            if (dist <= float.Epsilon)
                return;

            float2 dir = delta / dist;

            // Expand target by halfSize của moving box
            float2 expand = halfSize;

            AABB expanded = new AABB(
                target.min - expand,
                target.max + expand
            );

            // Initial overlap
            if (GeometryAABB.Contains(expanded, prevCenter))
            {
                hit.hit = true;
                hit.length = 0f;
                hit.point = prevCenter;
                hit.normal = GeometryAABB.ComputeAABBNormal(prevCenter, expanded);
                return;
            }

            Ray ray = new Ray(prevCenter, dir);
            GeometryRaycast.Raycast(ray, dist, expanded, out hit);
        }
        
        public static void SweepAABBCircle(float2 prevCenter, float2 currCenter, float2 halfSize, Circle targetCircle, out RayHit2D hit)
        {
            hit = default;

            float2 delta = currCenter - prevCenter;
            float dist = math.length(delta);

            if (dist <= float.Epsilon) return;

            float2 dir = delta / dist;

            // 1. Tạo một AABB ảo bao quanh Circle, được mở rộng bởi halfSize của AABB di động
            // Đây là vùng "tiềm năng" va chạm.
            AABB expandedBox = new AABB(
                targetCircle.center - (targetCircle.radius + halfSize),
                targetCircle.center + (targetCircle.radius + halfSize)
            );

            // 2. Raycast với AABB mở rộng này để tìm khoảng thời gian t sơ bộ
            Ray ray = new Ray(prevCenter, dir);
            GeometryRaycast.Raycast(ray, dist, expandedBox, out hit);
            
            GeometryGizmos.DrawRay(ray, Color.white, Time.deltaTime, dist);
            GeometryGizmos.DrawAABB(expandedBox, Color.white, Time.deltaTime);
        }

        static float2 WorldToOBBLocal(float2 p, OBB obb)
        {
            float2 d = p - obb.center;
            return new float2(math.dot(d, obb.axisX), math.dot(d, obb.axisY));
        }
        
        static AABB OBBToLocalAABB(OBB obb) => new AABB(-obb.halfSize, obb.halfSize);
        
        static float2 OBBLocalToWorld(float2 p, OBB obb) => obb.center + p.x * obb.axisX + p.y * obb.axisY;

        static float2 DirFromOBBLocal(float2 d, OBB obb) => d.x * obb.axisX + d.y * obb.axisY;
    }
}