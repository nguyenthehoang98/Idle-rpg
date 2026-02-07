using System;
using Geometry;
using Geometry.Primary;
using Unity.Mathematics;
using UnityEngine;
using Ray = Geometry.Ray;

namespace _Game.AbilitySystem
{
    public sealed class ShapeLogic: IDisposable
    {
        private readonly ShapeInstance shapeInstance;
        private float2 prevPos;

        public ShapeLogic(Shape data)
        {
            switch (data.type)
            {
                case ShapeType.Box:
                    shapeInstance = ShapeInstance.Insert(ShapeType.Box, data.size);
                    break;
                case ShapeType.Circle:
                    shapeInstance = ShapeInstance.Insert(ShapeType.Circle, data.radius);
                    break;
                default:
#if UNITY_EDITOR
                    string message = "Not define ShapeType: " + data.type;
                    throw new NotImplementedException(message);     
#endif
                    shapeInstance = ShapeInstance.Empty;
                    break;
            }
        }

        public void Startup(float2 pos)
        {
            prevPos = pos;
        }

        public void PreExecute()
        {
            
        }

        public void Execute(float2 center, ShapeInstance other, float2 otherPos, float deltaTime, out bool hit)
        {
            float2 prev = prevPos;
            float2 cur = center;
            RayHit2D hit2D = default;
            
            bool overlaps = GeometryUtils.Overlaps(
                shapeInstance, prev, cur,
                other, otherPos);
            if (!overlaps)
            {
                GeometryUtils.Sweep(
                    shapeInstance, prev, cur,
                    other, otherPos,
                    out hit2D);
                hit = hit2D.hit;
            }
            else
            {
                hit = true;
            }

#if UNITY_EDITOR
            if (hit)
            {
                if (overlaps)
                {
                    GeometryGizmos.DrawAABB(
                        AABB.FromCenter(hit2D.point, new float2(0.5f, 0.5f)),
                        new Color(1, 0, 0, 1), deltaTime
                    );
                    Box2dSelected(hit2D.point, new float2(0.5f, 0.5f),
                        new Color(1, 0, 0, 1), deltaTime
                    );
                }
                Debug.DrawLine((Vector2) prev, (Vector2) cur,
                    new Color(1, 0, 0, 1), deltaTime
                );
            }
            else
            {
                GeometryGizmos.DrawCircle(
                    new Circle(otherPos, other.Radius), Color.cyan, deltaTime * 2
                );
                GeometryGizmos.DrawAABB(
                    AABB.FromCenter(prev, new float2(0.35f, 0.35f)),
                    new Color(0.5f, 0, 1f, 0.5f), deltaTime * 2
                );
                Debug.DrawLine((Vector2) prev, (Vector2) cur,
                    new Color(0.5f, 0, 1f, 0.5f), deltaTime * 2
                );
            }
            
#endif
        }

        public void AfterExecute(float2 center)
        {
            prevPos = center;
        }

        public void Shutdown()
        {
            ShapeInstance.Remove(shapeInstance);
        }

        public void Dispose()
        {
        }
        
        static void Box2dSelected(float2 center, float2 size, Color color, float deltaTime)
        {
            float2 half = size * 0.5f;

            Vector3 p1 = new Vector3(center.x - half.x, center.y - half.y, 0);
            Vector3 p2 = new Vector3(center.x + half.x, center.y - half.y, 0);
            Vector3 p3 = new Vector3(center.x + half.x, center.y + half.y, 0);
            Vector3 p4 = new Vector3(center.x - half.x, center.y + half.y, 0);

            Debug.DrawLine(p1, p3, color, deltaTime);
            Debug.DrawLine(p2, p4, color, deltaTime);
        }
    }
}