using System;
using Geometry;
using Unity.Mathematics;

namespace _Game.AbilitySystem
{
    [Serializable]
    public struct ShapeData
    {
        public ShapeType type;
        public float radius;
        public float2 size;
    }
}