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
#if UNITY_EDITOR
                    string message = "Not define ShapeType: " + data.type;
                    throw new NotImplementedException(message);     
#endif
                    shape = Shape.Empty();
                    break;
            }
        }

        public void Execute(Vector2 center, Shape other, float deltaTime, out RayHit2D hit2D)
        {
            shape.PrefPosition = shape.CurrentPosition;
            shape.CurrentPosition = center;
            GeometryUtils.Overlaps(shape, other, out hit2D);
        }

        public void Dispose()
        {
            Shape.Remove(shape);
        }
    }
}