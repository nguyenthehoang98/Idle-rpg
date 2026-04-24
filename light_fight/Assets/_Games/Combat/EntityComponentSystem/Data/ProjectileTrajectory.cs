using Unity.Entities;
using Unity.Mathematics;

namespace _Games.Combat.EntityComponentSystem.Data
{
    public struct ProjectileTrajectory : IComponentData
    {
        public readonly Entity Target;
        public float3 StartPosition;
        public readonly float3 EndPosition;
        public readonly float3 Direction;
        public double ElapsedTime;

        public ProjectileTrajectory(Entity target, float3 startPosition, float3 endPosition, float3 direction)
        {
            Target = target;
            StartPosition = startPosition;
            EndPosition = endPosition;
            Direction = direction;
            ElapsedTime = 0;
        }
    }
}