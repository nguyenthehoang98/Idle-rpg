using System.Collections.Generic;
using _BattleSource.Entity;
using _Game.Configs;
using _KITSystem.Entity;
using UnityEngine;

namespace _Game.GamePlay.Manager
{
    public static class MonsterEntityManager
    {
        private static Dictionary<int, int> agentToEntity = new Dictionary<int, int>();
        private static Dictionary<int, int> entityToAgent = new Dictionary<int, int>();
        
        public static int CreateEntity(int agent, MonsterRuntimeData runtimeData, MonsterData monsterData)
        {
            int entity = EntityManager.NewEntity();
            
            int health = Mathf.CeilToInt(monsterData.health * runtimeData.HealthScale);
            int attack = Mathf.CeilToInt(monsterData.attack * runtimeData.AttackScale);
            int exp = Mathf.CeilToInt(monsterData.exp * runtimeData.ExpScale);
            
            ComponentManager<HealthData>.Add(entity, new HealthData(health));
            ComponentManager<StatData>.Add(entity, new StatData(exp, attack));
            
            agentToEntity.Add(agent, entity);
            entityToAgent.Add(entity, agent);

            return entity;
        }

        public static bool TryGetAgent(int entity, out int agent)
        {
            return entityToAgent.TryGetValue(entity, out agent);
        }

        public static bool TryGetEntity(int agent, out int entity)
        {
            return agentToEntity.TryGetValue(agent, out entity);
        }
    }
}