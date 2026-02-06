using System;
using Unity.Mathematics;

namespace Geometry
{
    [Serializable]
    public struct Shape
    {
        public ShapeType type;
        public float radius;
        public float2 size;
    }
}