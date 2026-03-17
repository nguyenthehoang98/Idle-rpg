using Unity.Entities;
using UnityEngine;

namespace _Games.Combat.EntityComponentSystem.View
{
    public class EntityView : MonoBehaviour
    {
        protected Entity m_Entity;

        public bool IsEntityCreated => m_Entity != Entity.Null;

        public Entity GetOrCreateEntity()
        {
            if (m_Entity != Entity.Null)
                return m_Entity;

            var world = World.DefaultGameObjectInjectionWorld;
            var manager = world.EntityManager;
            m_Entity = manager.CreateEntity();
            return m_Entity;
        }

        public void OnDestroy()
        {
            var world = World.DefaultGameObjectInjectionWorld;
            if (world == null || m_Entity == Entity.Null)
                return;

            var manager = world.EntityManager;
            manager.DestroyEntity(m_Entity);

            m_Entity = Entity.Null;
        }

        void OnEnable()
        {
            var world = World.DefaultGameObjectInjectionWorld;
            if (world == null || m_Entity == Entity.Null)
                return;

            var manager = world.EntityManager;
            manager.SetEnabled(m_Entity, true);
        }

        void OnDisable()
        {
            var world = World.DefaultGameObjectInjectionWorld;
            if (world == null || m_Entity == Entity.Null)
                return;

            var manager = world.EntityManager;
            manager.SetEnabled(m_Entity, false);
        }
    }
}