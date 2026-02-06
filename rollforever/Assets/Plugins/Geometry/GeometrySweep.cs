using Unity.Mathematics;

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

            if (GeometryAABB.Contains(expanded, prevCenter))
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