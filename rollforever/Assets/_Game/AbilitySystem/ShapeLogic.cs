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

        public void Execute(Vector2 center, Shape other, float deltaTime, out RayHit2D hit2D)
        {
            hit2D = new RayHit2D();
            if (shape.Type == ShapeType.Box && other.Type == ShapeType.Box)
            {
                hit2D.hit = GeometryAABB.Overlaps(
                    AABB.FromCenter(center, shape.Size),
                    AABB.FromCenter(other.CurrentPosition, other.Size)
                );
            }
            else if (shape.Type == ShapeType.Circle && other.Type == ShapeType.Circle)
            {
                hit2D.hit = GeometryCircle.Overlaps(
                    new Circle(center, shape.Radius),
                    new Circle(other.CurrentPosition, other.Radius)
                );
            }
            else if (shape.Type == ShapeType.Box && other.Type == ShapeType.Circle)
            {
                hit2D.hit = GeometryAABB.Overlaps(
                    AABB.FromCenter(center, shape.Size),
                    new Circle(other.CurrentPosition, other.Radius)
                );
            }
            else if (shape.Type == ShapeType.Circle && other.Type == ShapeType.Box)
            {
                hit2D.hit = GeometryCircle.Overlaps(
                    new Circle(center, shape.Radius),
                    AABB.FromCenter(other.CurrentPosition, other.Size)
                );
            }
        }

        public void Dispose()
        {
            Shape.Remove(shape);
        }
    }
}