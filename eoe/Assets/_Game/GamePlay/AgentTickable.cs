using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using _BattleSource.Entity;
using _Game.Configs;
using _KITSystem.Entity;
using _KITSystem.Grid;
using _KITSystem.Schedule;
using Unity.Mathematics;
using UnityEngine;

namespace _Game.GamePlay
{
    [Serializable]
    public class AgentTickable : AgentSimulator, ITickable
    {
        public float stopDistance = 1.6f;
        private List<Data> list = new List<Data>();
        private Dictionary<Monster, Data> monsterToData = new Dictionary<Monster, Data>();
        private Dictionary<int, int> agentToEntity = new Dictionary<int, int>();
        private Dictionary<int, Monster> entityToMonster = new Dictionary<int, Monster>();
        private Queue<Data> additionalQueue = new Queue<Data>();
        private Queue<Data> removeQueue = new Queue<Data>();
        
        private static AgentTickable instance;
        
        public new Task Initialize()
        {
            base.Initialize();
            instance = this;
            return Task.CompletedTask;
        }

        public override void Tick(float deltaTime)
        {
            base.Tick(deltaTime);

            while (additionalQueue.Count > 0)
            {
                list.Add(additionalQueue.Dequeue());
            }

            for (int i = list.Count - 1; i >= 0; i--)
            {
                Data data = list[i];

                if (data != null)
                {
                    if (TryGetAgent(data.Agent, out AgentData agent))
                    {
                        data.Monster.SetPosition(new Vector3(agent.position.x, agent.position.y), deltaTime);
                    }
                }
                else
                {
                    list.RemoveAt(i);
                }
            }

            while (removeQueue.Count > 0)
            {
                Data item = removeQueue.Dequeue();
                
                DestroyAgent(item.Agent);
                
                item.Monster.Destroy();
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            instance = null;
        }

        public static void Add(Monster monster, MonsterData monsterData) => instance.AddPrivate(monster, monsterData);

        public static bool TryGetMonster(int entity, out Monster monster)
        {
            return instance.entityToMonster.TryGetValue(entity, out monster);
        }

        public static int Query(float2 position, float2 size, out AgentData[] agentsData)
        {
            return instance.QueryAgent(position, size, out agentsData);
        }
        
        public static void Remove(Monster monster) => instance.RemovePrivate(monster);

        public static void Remove(int entity) => instance.RemovePrivate(entity);

        public static int GetEntity(int agent)
        {
            return instance.agentToEntity.GetValueOrDefault(agent, -1);
        }

        void AddPrivate(Monster monster, MonsterData monsterData)
        {
            int agent = CreateAgent(
                monster.transform.position, monsterData.radius, monsterData.speed,
                stopDistance + monsterData.stopDistance).agent;
            Data data = new Data(monster, agent);
            int entity = EntityManager.NewEntity();
            
            ComponentManager<HealthData>.Add(entity, new HealthData(100));
            
            agentToEntity.Add(agent, entity);
            monsterToData.Add(monster, data);
            entityToMonster.Add(entity, monster);
            additionalQueue.Enqueue(data);
        }
        
        void RemovePrivate(Monster monster)
        {
            if (monsterToData.Remove(monster, out Data data))
            {
                removeQueue.Enqueue(data);
                agentToEntity.Remove(data.Agent);
            }
        }
        
        void RemovePrivate(int entity)
        {
            if (entityToMonster.Remove(entity, out Monster monster))
            {
                RemovePrivate(monster);
            }
        }

        class Data
        {
            public readonly Monster Monster;
            public readonly int Agent;

            public Data(Monster monster, int agent)
            {
                Monster = monster;
                Agent = agent;
            }
        }
    }
}