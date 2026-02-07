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
                    shapeInstance = ShapeInstance.Create(ShapeType.Box, data.size);
                    break;
                case ShapeType.Circle:
                    shapeInstance = ShapeInstance.Create(ShapeType.Circle, data.radius);
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
            
            hit = GeometryUtils.Overlaps(
                shapeInstance, prev, cur,
                other, otherPos);
            if (!hit)
            {
                hit = GeometryUtils.Sweep(
                    shapeInstance, prev, cur,
                    other, otherPos);
            }
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