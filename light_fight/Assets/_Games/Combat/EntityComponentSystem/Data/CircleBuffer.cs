using Unity.Entities;
using Unity.Mathematics;

namespace _Games.Combat.EntityComponentSystem.Data
{
    public struct CircleBuffer : IBufferElementData
    {
        public readonly float Radius;
        public readonly float3 Offset;
        public readonly bool AdjustRadius;
        public readonly float ExtraRadius;

        public CircleBuffer(float radius, float3 offset, bool adjustRadius, float extraRadius)
        {
            Radius = radius;
            Offset = offset;
            AdjustRadius = adjustRadius;
            ExtraRadius = extraRadius;
        }
    }
}