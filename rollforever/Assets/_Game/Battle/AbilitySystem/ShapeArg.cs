using System;
using Geometry.Primary;
using Unity.Mathematics;
using UnityEngine;

namespace _Game.Battle.AbilitySystem
{
    [Serializable]
    public struct ShapeArg
    {
        public ShapeType type;
        public float2 size;
        public float radius;
        public bool customValue;
        public AnimationCurve curve;
        public float extraRadius;
        public float2 extraSize;
        public float duration;
    }
}