using Unity.Entities;
using Unity.Mathematics;

namespace _Games.Combat.EntityComponentSystem.Data
{
    public struct CircleBuffer : IBufferElementData
    {
        public readonly float Radius;
        public readonly float3 Offset;

        public CircleBuffer(float radius, float3 offset)
        {
            Radius = radius;
            Offset = offset;
        }
    }
}