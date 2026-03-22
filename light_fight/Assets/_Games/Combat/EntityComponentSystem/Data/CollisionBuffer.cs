using Unity.Entities;

namespace _Games.Combat.EntityComponentSystem.Data
{
    public struct CollisionBuffer : IBufferElementData
    {
        public Entity Entity;
        public bool OnTrigger;

        public CollisionBuffer(Entity entity)
        {
            Entity = entity;
            OnTrigger = false;
        }
    }
}