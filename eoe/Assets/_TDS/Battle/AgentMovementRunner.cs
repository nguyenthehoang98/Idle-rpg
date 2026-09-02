using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using _GameToolkit.Avoidance;
using _GameToolkit.Entities;
using _GameToolkit.Updater;
using _TDS.GameConfig;
using Unity.Mathematics;
using UnityEngine;

namespace _TDS.Battle
{
    [Serializable]
    public sealed class AgentMovementRunner : TickRunner
    {
        [SerializeField] private float stopDistance = 1.6f;
        [SerializeField] private AgentSimulator simulator;
        
        private Dictionary<int, Temp> container = new Dictionary<int, Temp>();
        private List<Temp> list = new List<Temp>();
        private Queue<Temp> additional = new Queue<Temp>();
        private Queue<Temp> remove = new Queue<Temp>();

        public void Initialize()
        {
            simulator.Initialize();
        }

        public override void Tick(float deltaTime)
        {
            simulator.Tick(deltaTime);

            while (additional.Count > 0) list.Add(additional.Dequeue());

            for (int i = list.Count - 1; i >= 0; i--)
            {
                Temp temp = list[i];
                
                if (temp == null) list.RemoveAt(i);
                else
                {
                    if (simulator.TryGetAgent(temp.Agent, out var agent))
                    {
                        Vector3 position = new Vector3(agent.position.x, agent.position.y);
                        
                        temp.Monster.SetPosition(position, deltaTime);
                    }
                }
            }
            
            while (remove.Count > 0)
            {
                Temp temp = remove.Dequeue();
                simulator.DestroyAgent(temp.Agent);
                Destroy_Agent(temp.Agent);
            }
        }

        public void Dispose()
        {
            simulator.Dispose();
        }

        public void Create_Agent(Monster monster, SpawnScaleDefinition scaleDefinition, MonsterConfigData monsterConfigData)
        {
            int agent = simulator.CreateAgent(
                monster.transform.position, monster.Radius, monsterConfigData.moveSpeed,
                stopDistance + monsterConfigData.stopDistance
            ).agent;
            
            monster.Initialize(scaleDefinition);
            
            Temp temp = new Temp(monster, agent);
            
            additional.Enqueue(temp);
            
            container.Add(agent, temp);

            int health = Mathf.CeilToInt(monsterConfigData.health * scaleDefinition.healthMultiplier);
            int attack = Mathf.CeilToInt(monsterConfigData.attack * scaleDefinition.attackMultiplier);
            int exp = Mathf.CeilToInt(monsterConfigData.exp * scaleDefinition.expMultiplier);
            
            ComponentManager<HealthData>.Add(agent, new HealthData(health));
        }

        public int Query_Agent(float2 position, float2 size, out AgentData[] agentsData)
        {
            return simulator.QueryAgent(position, size, out agentsData);
        }

        public bool TryGet_AgentPosition(int agent, out Vector3 position)
        {
            if (simulator.TryGetAgent(agent, out AgentData agentData))
            {
                position = new Vector3(agentData.position.x, agentData.position.y);
                return true;
            }
            else
            {
                position = Vector3.zero;
                return false;
            }
        }
        
        public bool TryGet_Monster(int agent, out Monster monster)
        {
            if (container.TryGetValue(agent, out Temp temp))
            {
                monster = temp.Monster;
                return true;
            }
            else
            {
                monster = null;
                return false;
            }
        }

        public void Destroy_Agent(int agent, ref Action onDestroyMonsterCommand)
        {
            if (container.Remove(agent, out Temp temp))
            {
                Monster m = temp.Monster;
                onDestroyMonsterCommand += () => { m.Death(); };
                
                remove.Enqueue(temp);
            }
        }

        public void Destroy_Agent(int agent)
        {
            if (container.Remove(agent, out Temp temp))
            {
                Monster m = temp.Monster;
                m.Death();
                remove.Enqueue(temp);
            }
        }
        
        class Temp
        {
            public Monster Monster;
            public int Agent;

            public Temp(Monster monster, int agent)
            {
                Monster = monster;
                Agent = agent;
            }
        }
    }
}