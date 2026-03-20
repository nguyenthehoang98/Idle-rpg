using Unity.Entities;
using Unity.Mathematics;

namespace _Games.Combat.EntityComponentSystem.Model
{
    public struct RangedCastSkillData
    {
        public Entity Entity;
        public float3 StartPosition;
        public float3 EndPosition;

        public RangedCastSkillData(Entity entity, float3 startPosition, float3 endPosition)
        {
            Entity = entity;
            StartPosition = startPosition;
            EndPosition = endPosition;
        }
    }
}