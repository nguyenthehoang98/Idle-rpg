using System;
using Geometry;
using Unity.Mathematics;
using UnityEngine;

namespace _Game.AbilitySystem
{
    public sealed class ShapeLogic: IDisposable
    {
        private readonly ShapeInstance shapeInstance;

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

        public void Execute(Vector2 center, ShapeInstance other, float deltaTime, out RayHit2D hit2D)
        {
            shapeInstance.PrefPosition = shapeInstance.CurrentPosition;
            shapeInstance.CurrentPosition = center;

            bool overlaps = GeometryUtils.Overlaps(shapeInstance, other, out hit2D);
            if (!overlaps)
            {
                GeometryUtils.Sweep(shapeInstance, other, out hit2D);                
            }
            
#if UNITY_EDITOR
            if (hit2D.hit)
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
                Debug.DrawLine((Vector2) shapeInstance.PrefPosition, (Vector2) shapeInstance.CurrentPosition,
                    new Color(1, 0, 0, 1), deltaTime
                );
            }
            else
            {
                GeometryGizmos.DrawCircle(
                    new Circle(other.CurrentPosition, other.Radius), Color.cyan, deltaTime * 2
                );
                GeometryGizmos.DrawAABB(
                    AABB.FromCenter(shapeInstance.PrefPosition, new float2(0.35f, 0.35f)),
                    new Color(0.5f, 0, 1f, 0.5f), deltaTime * 2
                );
                Debug.DrawLine((Vector2) shapeInstance.PrefPosition, (Vector2) shapeInstance.CurrentPosition,
                    new Color(0.5f, 0, 1f, 0.5f), deltaTime * 2
                );
            }
#endif
        }

        public void Dispose()
        {
            ShapeInstance.Remove(shapeInstance);
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