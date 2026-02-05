using System;
using _Game.Battle.Data;
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

        public void Dispose()
        {
            Shape.Remove(shape);
        }
    }
}