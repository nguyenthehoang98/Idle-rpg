using System;
using Unity.Mathematics;

namespace Geometry.Primary
{
    [Serializable]
    public struct Shape
    {
        public ShapeType type;
        public float2 size;
        public float radius;
    }

    public enum ShapeType
    {
        Circle, Box,
    }
}