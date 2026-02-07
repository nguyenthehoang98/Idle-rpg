using Unity.Mathematics;
using UnityEngine;

namespace Geometry.Primary
{
    public readonly struct Circle
    {
        public readonly float2 center;
        public readonly float radius;

        public Circle(float2 center, float radius)
        {
            this.center = center;
            this.radius = radius;
        }
    }
}