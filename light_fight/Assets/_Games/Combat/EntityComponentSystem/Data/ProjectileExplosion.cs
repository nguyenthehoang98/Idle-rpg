using Unity.Entities;
using Unity.Mathematics;

namespace _Games.Combat.EntityComponentSystem.Data
{
    public struct ProjectileExplosion : IComponentData
    {
        public readonly float Radius;
        public readonly float3 OffsetCenter;
        public readonly float Duration;
        public bool OnTriggerModifier;
        public double ElapsedTime;

        public ProjectileExplosion(float radius, float3 offsetCenter, float duration)
        {
            Radius = radius;
            OffsetCenter = offsetCenter;
            Duration = duration;
            OnTriggerModifier = false;
            ElapsedTime = 0.0;
        }
    }
}