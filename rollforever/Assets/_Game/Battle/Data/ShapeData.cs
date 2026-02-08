using Geometry.Primary;
using Unity.Mathematics;

namespace _Game.Battle.Data
{
    public readonly struct ShapeData
    {
        public Shape Value { get; }

        private ShapeData(Shape shape) => Value = shape;

        public static ShapeData Circle(float radius)
        {
            return new ShapeData(new Shape {type = ShapeType.Circle, radius = radius});
        }

        public static ShapeData Box(float2 size)
        {
            return new ShapeData(new Shape {type = ShapeType.Box, size = size});
        }
    }
}