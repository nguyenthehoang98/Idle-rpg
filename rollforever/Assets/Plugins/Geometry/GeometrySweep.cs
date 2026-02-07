using Geometry.Primary;
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

        public static void SweepCircleOBB(float2 prevCenter, float2 currCenter, float radius, OBB obb, out RayHit2D hit)
        {
            hit = default;

            // 1. Chuyển đổi vị trí từ World Space sang Local Space của OBB
            // Vector từ tâm OBB tới điểm cần chuyển
            float2 relPrev = prevCenter - obb.center;
            float2 relCurr = currCenter - obb.center;

            // Chiếu lên các trục của OBB để tìm tọa độ Local
            float2 localPrev = new float2(math.dot(relPrev, obb.axisX), math.dot(relPrev, obb.axisY));
            float2 localCurr = new float2(math.dot(relCurr, obb.axisX), math.dot(relCurr, obb.axisY));

            // 2. Tạo AABB đại diện cho OBB trong Local Space (nằm tại gốc tọa độ)
            AABB localAABB = new AABB(-obb.halfSize, obb.halfSize);

            // 3. Gọi hàm Sweep với logic xử lý góc bo tròn
            if (SweepCircleAABB_Internal(localPrev, localCurr, radius, localAABB, out hit))
            {
                hit.normal = hit.normal.x * obb.axisX + hit.normal.y * obb.axisY;
                hit.point = (hit.point.x * obb.axisX + hit.point.y * obb.axisY) + obb.center;
            }
        }
        
        private static bool SweepCircleAABB_Internal(float2 prev, float2 curr, float radius, AABB box, out RayHit2D hit)
        {
            hit = default;
            float2 delta = curr - prev;
            float dist = math.length(delta);
            if (dist <= float.Epsilon) return false;

            float2 dir = delta / dist;
            Ray ray = new Ray(prev, dir);

            // Vùng 1: Các cạnh (Mở rộng AABB theo hình chữ thập)
            AABB expandX = new AABB(new float2(box.min.x - radius, box.min.y), new float2(box.max.x + radius, box.max.y));
            AABB expandY = new AABB(new float2(box.min.x, box.min.y - radius), new float2(box.max.x, box.max.y + radius));

            /*GeometryGizmos.DrawRay(ray, Color.white, Time.deltaTime, dist);
            GeometryGizmos.DrawAABB(expandX, Color.white, Time.deltaTime);
            GeometryGizmos.DrawAABB(expandY, Color.blue, Time.deltaTime);*/
            
            RayHit2D hX, hY;
            GeometryRaycast.Raycast(ray, dist, expandX, out hX);
            GeometryRaycast.Raycast(ray, dist, expandY, out hY);

            hit = hX.hit ? hX : hit;
            if (hY.hit && (!hit.hit || hY.length < hit.length)) hit = hY;

            // Vùng 2: 4 góc bo tròn (Mỗi góc là 1 Circle bán kính 'radius')
            float2[] corners = { box.min, new float2(box.max.x, box.min.y), new float2(box.min.x, box.max.y), box.max };
            foreach (float2 corner in corners)
            {
                RayHit2D hC;
                Circle c = new Circle(corner, radius);
                GeometryRaycast.Raycast(ray, dist, c, out hC);
                if (hC.hit && (!hit.hit || hC.length < hit.length)) hit = hC;
            }

            return hit.hit;
        }
        
        public static void SweepAABBOBB(float2 prevCenter, float2 currCenter, float2 aabbHalfSize, OBB target, out RayHit2D hit)
        {
            hit = default;

            // 1. Tính toán vector di chuyển (Ray)
            float2 delta = currCenter - prevCenter;
            float dist = math.length(delta);
            if (dist <= float.Epsilon) return;
            float2 dir = delta / dist;

            // 2. Chuyển bài toán về Local Space của OBB
            // Vector từ tâm OBB tới điểm bắt đầu
            float2 localOrigin = new float2(
                math.dot(prevCenter - target.center, target.axisX),
                math.dot(prevCenter - target.center, target.axisY)
            );

            // Hướng di chuyển trong Local Space
            float2 localDir = new float2(
                math.dot(dir, target.axisX),
                math.dot(dir, target.axisY)
            );

            // 3. Tính toán độ mở rộng (Minkowski Sum)
            // Chiếu các trục của AABB di động lên các trục của OBB mục tiêu
            float extX = math.abs(aabbHalfSize.x * target.axisX.x) + math.abs(aabbHalfSize.y * target.axisX.y);
            float extY = math.abs(aabbHalfSize.x * target.axisY.x) + math.abs(aabbHalfSize.y * target.axisY.y);
            float2 expandedHalfSize = target.halfSize + new float2(extX, extY);

            // 4. Thực hiện Slab Method (Raycast AABB) trong Local Space
            float tMin = 0f;
            float tMax = dist;

            // Slab trục X
            if (!Slab(localOrigin.x, localDir.x, -expandedHalfSize.x, expandedHalfSize.x, ref tMin, ref tMax)) return;
            // Slab trục Y
            if (!Slab(localOrigin.y, localDir.y, -expandedHalfSize.y, expandedHalfSize.y, ref tMin, ref tMax)) return;

            // 5. Tổng hợp kết quả
            hit.hit = true;
            hit.length = tMin;
            hit.point = prevCenter + dir * tMin; // Tâm của AABB tại thời điểm va chạm

            // 6. Tính toán Normal trong World Space
            // Xác định mặt nào của OBB bị chạm dựa trên vị trí va chạm local
            float2 localHitPos = localOrigin + localDir * tMin;
            float2 localNormal = float2.zero;

            // Tìm cạnh gần nhất để xác định Normal
            float minDiff = float.MaxValue;

            float dL = math.abs(localHitPos.x - (-expandedHalfSize.x));
            if (dL < minDiff) { minDiff = dL; localNormal = -target.axisX; }
            
            float dR = math.abs(localHitPos.x - expandedHalfSize.x);
            if (dR < minDiff) { minDiff = dR; localNormal = target.axisX; }
            
            float dB = math.abs(localHitPos.y - (-expandedHalfSize.y));
            if (dB < minDiff) { minDiff = dB; localNormal = -target.axisY; }
            
            float dT = math.abs(localHitPos.y - expandedHalfSize.y);
            if (dT < minDiff) { localNormal = target.axisY; }

            hit.normal = localNormal;
            
            // 7. (Tùy chọn) Điều chỉnh hit.point về điểm tiếp xúc thực tế trên bề mặt OBB
            // hit.point = hit.point - (hit.normal * (giá trị hình chiếu halfSize lên normal))
        }

        // Hàm hỗ trợ Slab tĩnh (Internal)
        private static bool Slab(float start, float dir, float min, float max, ref float tMin, ref float tMax)
        {
            if (math.abs(dir) < 1e-7f)
            {
                return start >= min && start <= max;
            }
            float t1 = (min - start) / dir;
            float t2 = (max - start) / dir;
            if (t1 > t2) { float tmp = t1; t1 = t2; t2 = tmp; }
            tMin = math.max(tMin, t1);
            tMax = math.min(tMax, t2);
            return tMin <= tMax;
        }
    }
}