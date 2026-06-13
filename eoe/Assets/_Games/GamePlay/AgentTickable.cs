using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using _Games.GamePlay.AnimationSystem;
using _KITSystem.Grid;
using _KITSystem.Resource;
using _KITSystem.Schedule;
using UnityEngine;

namespace _Games.GamePlay
{
    [Serializable]
    public class AgentTickable : AgentSimulator, ITickable
    {
        private List<Data> list = new List<Data>();
        private Dictionary<Monster, Data> container = new Dictionary<Monster, Data>();
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

                        if (agent.isStopped) RemovePrivate(data.Monster);
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

        public static void Remove(Monster unit) => instance.RemovePrivate(unit);

        void AddPrivate(Monster unit)
        {
            int agent = CreateAgent(unit.transform.position, 0.2f, 2, 3).agent;
            Data data = new Data(unit, agent);
            
            container.Add(unit, data);
            additionalQueue.Enqueue(data);
        }
        
        void RemovePrivate(Monster unit)
        {
            if (container.Remove(unit, out Data data)) removeQueue.Enqueue(data);
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