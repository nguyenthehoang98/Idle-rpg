using System.Collections.Generic;
using _KITSystem.Grid;
using _KITSystem.SkillSystem.Entity;

namespace _Games.Battle
{
    public static class EntityFactory
    {
        private static Dictionary<int, int> agentToEntity;
        private static Dictionary<int, int> entityToAgent;
        private static AgentGrid agentGrid;
        
        public static void Initialize(AgentGrid ag)
        {
            agentGrid = ag;
            entityToAgent = new Dictionary<int, int>();
            agentToEntity = new Dictionary<int, int>();
            EntityManager.OnEntityRemoved += EntityRemoved;
        }

        private static void EntityRemoved(int entity)
        {
            if(entityToAgent.Remove(entity, out int agentId))
            {
                agentGrid.DestroyAgent(agentId);
                agentToEntity.Remove(agentId);
            }
        }

        public static void CreateEntity(int agentId)
        {
            int entity = EntityManager.CreateEntity();
            EntityManager.AddComponent(entity, new HealthData(10));
            entityToAgent[entity] = agentId;
            agentToEntity[agentId] = entity;
        }

        public static bool FindEntity(int agent, out int entity)
        {
            return agentToEntity.TryGetValue(agent, out entity);
        }

        public static bool FindAgent(int entity, out int agent)
        {
            return entityToAgent.TryGetValue(entity, out agent);
        }
    }
}