using Geometry.Primary;
using Unity.Mathematics;
using UnityEngine;
using Ray = Geometry.Primary.Ray;

namespace Geometry.Math
{
    // sweep calculator
    // Xử lý kiểm tra va chạm 1 vật di chuyển với 1 vật đứng yên
    internal static partial class GeometryMath
    {
        internal static bool SweepCircleCircle(
            float2 aPrevPos, Circle aCircle, Circle bCircle
        )
        {
            float2 delta = aCircle.center - aPrevPos;
            float dist = math.length(delta);
            if (dist <= 0f)
                return false;

            return RayCircle(
                new Ray(aPrevPos, delta / dist),
                new Circle(bCircle.center, aCircle.radius + bCircle.radius),
                dist
            );
        }

        internal static bool SweepCircleBox(
            float2 prevPos, Circle circle, Box box
        )
        {
            float2 delta = circle.center - prevPos;
            float dist = math.length(delta);
            if (dist <= float.Epsilon)
                return false;

            float2 dir = delta / dist;
            Ray ray = new Ray(prevPos, dir);

            Box expandX = new Box(
                new float2(box.min.x - circle.radius, box.min.y),
                new float2(box.max.x + circle.radius, box.max.y)
            );
            Box expandY = new Box(
                new float2(box.min.x, box.min.y - circle.radius),
                new float2(box.max.x, box.max.y + circle.radius)
            );

            bool hitX = RayBox(ray, expandX, dist, out float xHitLength, out float2 xHitPoint);
            bool hitY = RayBox(ray, expandY, dist, out float yHitLength, out float2 yHitPoint);

            bool hit = false;
            float hitLength = 0;
            if (hitX)
            {
                hit = true;
                hitLength = xHitLength;
            }

            if (hitY && (!hit || yHitLength < hitLength))
            {
                hit = true;
                hitLength = yHitLength;
            }

            CheckCorner(box.min);
            CheckCorner(new float2(box.max.x, box.min.y));
            CheckCorner(new float2(box.min.x, box.max.y));
            CheckCorner(box.max);

            void CheckCorner(float2 cornerPos)
            {
                Circle c = new Circle(cornerPos, circle.radius);

                if (RayCircle(ray, c, dist, out float cornerT, out _))
                {
                    if (!hit || cornerT < hitLength)
                    {
                        hit = true;
                        hitLength = cornerT;
                    }
                }
            }

            return hit;
        }

        internal static bool SweepBoxBox(
            float2 aPrevPos, Box aBox, Box bBox
        )
        {
            float2 delta = aBox.center - aPrevPos;
            float dist = math.length(delta);

            if (dist <= EPSILON)
                return false;

            Box expanded = new Box(
                bBox.min - aBox.halfsize,
                bBox.max + aBox.halfsize
            );

            if (IsBoxContainPoint(expanded, aPrevPos))
            {
                return true;
            }

            float2 dir = delta / dist;
            return RayBox(new Ray(aPrevPos, dir), expanded, dist);
        }

        internal static bool SweepBoxCircle(
            float2 prevPos, Box box, Circle circle
        )
        {
            float2 delta = box.center - prevPos;
            float dist = math.length(delta);
            if (dist <= EPSILON)
                return false;

            float2 halfSize = box.halfsize;
            Box expandX = new Box(circle.center - new float2(circle.radius + halfSize.x, halfSize.y),
                circle.center + new float2(circle.radius + halfSize.x, halfSize.y));
            Box expandY = new Box(circle.center - new float2(halfSize.x, circle.radius + halfSize.y),
                circle.center + new float2(halfSize.x, circle.radius + halfSize.y));

            float2 dir = delta / dist;
            Ray ray = new Ray(prevPos, dir);

            bool hitX = RayBox(ray, expandX, dist, out float xHitLength, out float2 xHitPoint);
            bool hitY = RayBox(ray, expandY, dist, out float yHitLength, out float2 yHitPoint);

            bool hit = false;
            float hitLength = 0;
            if (hitX)
            {
                hit = true;
                hitLength = xHitLength;
            }

            if (hitY && (!hit || yHitLength < hitLength))
            {
                hit = true;
                hitLength = yHitLength;
            }
            
            CheckCorner(box.min);
            CheckCorner(new float2(box.max.x, box.min.y));
            CheckCorner(new float2(box.min.x, box.max.y));
            CheckCorner(box.max);

            void CheckCorner(float2 cornerPos)
            {
                float2 cornerPrev = cornerPos - delta;
                Ray cornerRay = new Ray(cornerPrev, dir);
                Circle c = new Circle(circle.center, circle.radius);
                if (RayCircle(cornerRay, c, dist, out float cornerT, out _))
                {
                    if (!hit || cornerT < hitLength)
                    {
                        hit = true;
                        hitLength = cornerT;
                    }
                }
            }

            return hit;
        }

        internal static bool SweepCircleObb(
            float2 prevPos, Circle circle, OBB obb
        )
        {
            float2 relPrev = prevPos - obb.center;
            float2 relCurr = circle.center - obb.center;

            float2 localPrev = new float2(math.dot(relPrev, obb.axisX), math.dot(relPrev, obb.axisY));
            float2 localCurr = new float2(math.dot(relCurr, obb.axisX), math.dot(relCurr, obb.axisY));

            Box box = new Box(-obb.halfSize, obb.halfSize);

            if (SweepCircleBox(localPrev, new Circle(localCurr, circle.radius), box))
            {
                return true;
            }

            return false;
        }

        internal static bool SweepBoxObb(
            float2 prevPos, Box box, OBB obb
        )
        {
            float2 delta = box.center - prevPos;
            float dist = math.length(delta);
            if (dist <= EPSILON)
                return false;
            
            float2 dir = delta / dist;
            float2 localDir = new float2(
                math.dot(dir, obb.axisX),
                math.dot(dir, obb.axisY)
            );
            float2 localOrigin = new float2(
                math.dot(prevPos - obb.center, obb.axisX),
                math.dot(prevPos - obb.center, obb.axisY)
            );

            float2 halfSize = box.halfsize;
            float extX = math.abs(halfSize.x * obb.axisX.x) + math.abs(halfSize.y * obb.axisX.y);
            float extY = math.abs(halfSize.x * obb.axisY.x) + math.abs(halfSize.y * obb.axisY.y);
            float2 expandedHalfSize = obb.halfSize + new float2(extX, extY);
            
            float tMin = 0f;
            float tMax = dist;

            if (!Slab(localOrigin.x, localDir.x, -expandedHalfSize.x, expandedHalfSize.x, ref tMin, ref tMax))
                return false;
            if (!Slab(localOrigin.y, localDir.y, -expandedHalfSize.y, expandedHalfSize.y, ref tMin, ref tMax))
                return false;

            return true;
        }
    }
}