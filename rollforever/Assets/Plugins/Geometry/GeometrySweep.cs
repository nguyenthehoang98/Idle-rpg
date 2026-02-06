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
            if (dist <= float.Epsilon) return;

            float2 dir = delta / dist;
            Ray ray = new Ray(prevCenter, dir);

            // 1. Tạo 2 box đại diện cho vùng "cạnh" (không bao gồm góc)
            // Box mở rộng theo trục X, giữ nguyên Y của box gốc
            AABB expandX = new AABB(new float2(box.min.x - radius, box.min.y), new float2(box.max.x + radius, box.max.y));
            // Box mở rộng theo trục Y, giữ nguyên X của box gốc
            AABB expandY = new AABB(new float2(box.min.x, box.min.y - radius), new float2(box.max.x, box.max.y + radius));

            /*GeometryGizmos.DrawRay(ray, Color.white, Time.deltaTime, dist);
            GeometryGizmos.DrawAABB(expandX, Color.white, Time.deltaTime);
            GeometryGizmos.DrawAABB(expandY, Color.blue, Time.deltaTime);*/
            
            // Raycast với 2 box này trước
            RayHit2D hitX, hitY;
            GeometryRaycast.Raycast(ray, dist, expandX, out hitX);
            GeometryRaycast.Raycast(ray, dist, expandY, out hitY);

            // Lấy va chạm gần nhất trong 2 box
            hit = hitX.hit ? hitX : hit;
            if (hitY.hit && (!hit.hit || hitY.length < hit.length)) hit = hitY;

            // 2. Xử lý 4 góc (Mỗi góc là 1 Circle bán kính 'radius' đặt tại đỉnh của AABB gốc)
            float2[] corners = new float2[] {
                box.min,
                new float2(box.max.x, box.min.y),
                new float2(box.min.x, box.max.y),
                box.max
            };

            foreach (float2 cornerPos in corners)
            {
                RayHit2D hitCorner;
                Circle c = new Circle(cornerPos, radius);
                GeometryRaycast.Raycast(ray, dist, c, out hitCorner);

                if (hitCorner.hit && (!hit.hit || hitCorner.length < hit.length))
                {
                    hit = hitCorner;
                }
            }
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
            Ray ray = new Ray(prevCenter, dir);

            // 1. Tạo AABB trung tâm (vùng va chạm cạnh)
            // Mở rộng targetCircle theo trục X bởi halfSize.x và trục Y bởi halfSize.y
            AABB boxX = new AABB(targetCircle.center - new float2(targetCircle.radius + halfSize.x, halfSize.y), 
                                 targetCircle.center + new float2(targetCircle.radius + halfSize.x, halfSize.y));
            
            AABB boxY = new AABB(targetCircle.center - new float2(halfSize.x, targetCircle.radius + halfSize.y), 
                                 targetCircle.center + new float2(halfSize.x, targetCircle.radius + halfSize.y));

            /*GeometryGizmos.DrawRay(ray, Color.white, Time.deltaTime, dist);
            GeometryGizmos.DrawAABB(boxX, Color.white, Time.deltaTime);
            GeometryGizmos.DrawAABB(boxY, Color.blue, Time.deltaTime);*/
            
            // 2. Kiểm tra va chạm với các vùng cạnh thẳng
            RayHit2D hitX, hitY;
            GeometryRaycast.Raycast(ray, dist, boxX, out hitX);
            GeometryRaycast.Raycast(ray, dist, boxY, out hitY);

            // Chọn va chạm gần nhất từ 2 box
            hit = hitX.hit ? hitX : hit;
            if (hitY.hit && (!hit.hit || hitY.length < hit.length)) hit = hitY;

            // 3. Kiểm tra va chạm với 4 góc (mỗi góc là một Circle bán kính targetCircle.radius)
            // Tâm của 4 góc này chính là 4 góc của một AABB có kích thước halfSize
            float2[] corners = new float2[] {
                targetCircle.center + new float2(halfSize.x, halfSize.y),
                targetCircle.center + new float2(-halfSize.x, halfSize.y),
                targetCircle.center + new float2(halfSize.x, -halfSize.y),
                targetCircle.center + new float2(-halfSize.x, -halfSize.y)
            };

            foreach (var cornerPos in corners)
            {
                RayHit2D hitCorner;
                Circle cornerCircle = new Circle(cornerPos, targetCircle.radius);
                GeometryRaycast.Raycast(ray, dist, cornerCircle, out hitCorner);
                
                if (hitCorner.hit && (!hit.hit || hitCorner.length < hit.length))
                {
                    hit = hitCorner;
                }
            }
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