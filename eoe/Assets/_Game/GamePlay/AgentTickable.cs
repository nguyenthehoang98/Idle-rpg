using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using _KITSystem.Entity;
using _KITSystem.Grid;
using _KITSystem.Schedule;
using _KITSystem.Utils;
using Unity.Mathematics;
using UnityEngine;

namespace _Game.GamePlay
{
    [Serializable]
    public class AgentTickable : AgentSimulator, ITickable
    {
        private List<Data> list = new List<Data>();
        private Dictionary<Monster, Data> container = new Dictionary<Monster, Data>();
        private Dictionary<int, int> agentToEntity = new Dictionary<int, int>();
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
                        data.Monster.transform.position = new Vector3(agent.position.x, agent.position.y);

                        //if (agent.isStopped) RemovePrivate(data.Monster);
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

        public static void Add(Monster unit) => instance.AddPrivate(unit);

        public static int Query(float2 position, float2 size, out AgentData[] agentsData)
        {
            return instance.QueryAgent(position, size, out agentsData);
        }
        
        public static void Remove(Monster unit) => instance.RemovePrivate(unit);

        public static int GetEntity(int agent)
        {
            return instance.agentToEntity.GetValueOrDefault(agent, -1);
        }

        void AddPrivate(Monster unit)
        {
            int agent = CreateAgent(unit.transform.position, 0.2f, 2, RandomUtils.Range(1.6f, 2.6f)).agent;
            Data data = new Data(unit, agent);
            int entity = EntityManager.NewEntity();
            
            agentToEntity.Add(agent, entity);
            container.Add(unit, data);
            additionalQueue.Enqueue(data);
        }
        
        void RemovePrivate(Monster unit)
        {
            if (container.Remove(unit, out Data data))
            {
                removeQueue.Enqueue(data);
                agentToEntity.Remove(data.Agent);
            }
        }

        class Data
        {
            public Monster Monster;
            
            public int Agent;

            public Data(Monster monster, int agent)
            {
                Monster = monster;
                Agent = agent;
            }
        }
    }
}