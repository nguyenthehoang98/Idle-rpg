using Geometry.Primary;
using Unity.Mathematics;

namespace _Game.Battle.Data
{
    public readonly struct ShapeData
    {
        public Shape Value { get; }

        public ShapeData(ShapeType type, float radius)
        {
            Value = new Shape {type = type, radius = radius};
        }

        public ShapeData(ShapeType type, float2 size)
        {
            Value = new Shape {type = type, size = size};
        }
    }
}