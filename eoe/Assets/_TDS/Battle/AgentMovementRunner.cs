using System;
using System.Collections.Generic;
using _GameToolkit.Avoidance;
using _GameToolkit.Entities;
using _GameToolkit.Updater;
using _TDS.GameConfig;
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

        /// <summary>Số quái còn sống trên sân (container = agent chưa bị remove khi chết).</summary>
        public int AliveCount => container.Count;

        private void OnEnable()
        {
            Monster.OnMonsterDisable += RemoveAgent;
        }

        private void OnDisable()
        {
            Monster.OnMonsterDisable -= RemoveAgent;
        }

        // Monster chết -> bỏ agent khỏi simulator & container (không gọi lại Death)
        private void RemoveAgent(Monster monster)
        {
            int foundAgent = -1;
            Temp found = null;

            foreach (KeyValuePair<int, Temp> pair in container)
            {
                if (pair.Value.Monster != monster) continue;
                foundAgent = pair.Key;
                found = pair.Value;
                break;
            }

            if (foundAgent != -1)
            {
                container.Remove(foundAgent);
                remove.Enqueue(found); // Tick() sẽ gọi simulator.DestroyAgent
            }
        }

        public override void Tick(float deltaTime)
        {
            simulator.Tick(deltaTime);

            while (additional.Count > 0) list.Add(additional.Dequeue());

            for (int i = list.Count - 1; i >= 0; i--)
            {
                Temp temp = list[i];
                
                if (temp == null) { list.RemoveAt(i); continue; }

                // agent đã bị remove (monster chết) -> dọn khỏi list
                if (!container.ContainsKey(temp.Agent))
                {
                    list.RemoveAt(i);
                    continue;
                }

                if (simulator.TryGetAgent(temp.Agent, out var agent))
                {
                    Vector3 position = new Vector3(agent.position.x, agent.position.y);
                    
                    temp.Monster.SetPosition(position, deltaTime);
                }
            }
            
            while (remove.Count > 0)
            {
                Temp temp = remove.Dequeue();
                simulator.DestroyAgent(temp.Agent);
            }
        }

        public void Dispose()
        {
            simulator.Dispose();
        }

        public void Create_Agent(Monster monster, SpawnScaleDefinition scaleDefinition, MonsterConfigData monsterConfigData)
        {
            int health = Mathf.CeilToInt(monsterConfigData.health * scaleDefinition.healthMultiplier);
            int attack = Mathf.CeilToInt(monsterConfigData.attack * scaleDefinition.attackMultiplier);
            int exp = Mathf.CeilToInt(monsterConfigData.exp * scaleDefinition.expMultiplier);

            monster.SetCombatData(health, attack);

            int agent = simulator.CreateAgent(
                monster.transform.position, monster.Radius, monsterConfigData.moveSpeed,
                stopDistance + monsterConfigData.stopDistance
            ).agent;
            
            monster.Initialize(scaleDefinition);
            
            Temp temp = new Temp(monster, agent);
            
            additional.Enqueue(temp);
            
            container.Add(agent, temp);

            ComponentManager<HealthData>.Add(agent, new HealthData(health));
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