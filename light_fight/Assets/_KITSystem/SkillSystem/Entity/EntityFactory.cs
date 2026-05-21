using System.Collections.Generic;

namespace _KITSystem.SkillSystem.Entity
{
    public static class EntityFactory
    {
        private static Dictionary<int, int> container;
        
        public static void Initialize()
        {
            container = new Dictionary<int, int>();
        }
        
        public static void NewEntity(int agent)
        {
            int entity = EntityManager.NewEntity();
            EntityManager.AddComponent(entity, new HealthData(10));
        }

        public static void RemoveEntity(int agent)
        {
            if (container.TryGetValue(agent, out int entity))
            {
                EntityManager.DestroyEntity(entity);
            }
        }
    }
}