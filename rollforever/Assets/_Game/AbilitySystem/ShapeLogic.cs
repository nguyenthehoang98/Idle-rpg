using System;
using _Game.Battle.Data;
using Geometry;
using UnityEngine;

namespace _Game.AbilitySystem
{
    public sealed class ShapeLogic: IDisposable
    {
        private readonly Shape shape;

        public ShapeLogic(ShapeData data)
        {
            switch (data.type)
            {
                case ShapeType.Box:
                    shape = Shape.Insert(ShapeType.Box, data.size);
                    break;
                case ShapeType.Circle:
                    shape = Shape.Insert(ShapeType.Circle, data.radius);
                    break;
                default:
                    Debug.LogError("Chưa định nghĩa");
                    break;
            }
        }

        public void Execute(Vector2 center, Shape other, float deltaTime)
        {
            if (shape.Type == ShapeType.Box && other.Type == ShapeType.Box)
            {
                
            }
            else if (shape.Type == ShapeType.Circle && other.Type == ShapeType.Circle)
            {
                
            }
            else if (shape.Type == ShapeType.Box && other.Type == ShapeType.Circle)
            {
                
            }
            else if (shape.Type == ShapeType.Circle && other.Type == ShapeType.Box)
            {
                
            }


            
        }

        public void Dispose()
        {
            Shape.Remove(shape);
        }
    }
}