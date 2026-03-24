using Unity.Entities;
using UnityEngine;

namespace _Games.Combat.EntityComponentSystem.View
{
    public class EntityView : MonoBehaviour
    {
        protected Entity m_Entity;

        public Entity GetOrCreateEntity()
        {
            if (m_Entity != Entity.Null)
                return m_Entity;

            var world = World.DefaultGameObjectInjectionWorld;
            var manager = world.EntityManager;
            m_Entity = manager.CreateEntity();
            return m_Entity;
        }
    }
}