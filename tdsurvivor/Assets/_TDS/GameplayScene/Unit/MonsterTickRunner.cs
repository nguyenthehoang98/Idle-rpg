using System;
using System.Collections.Generic;
using _TDS.Config;
using _Toolkit.Avoidance;
using _Toolkit.Updater;
using Unity.Mathematics;
using UnityEngine;

namespace _TDS.GameplayScene.Unit
{
    public class MonsterTickRunner : BaseTickRunner<MonsterTickRunner.Data>
    {
        [SerializeField] float stopDistance;
        [SerializeField] AgentSimulator simulator;

        public static MonsterTickRunner Instance { get; private set; }
        
        Dictionary<int, Data> agentToData = new Dictionary<int, Data>();

        public event Action OnMonsterRemoved;

        private void Awake()
        {
            Instance = this;

            simulator.Initialize();
        }

        public override void Tick(float deltaTime)
        {
            simulator.Tick(deltaTime);
            
            base.Tick(deltaTime);
        }

        private void OnDestroy()
        {
            Instance = null;
        }

        protected override void OnRemove(Data item)
        {
            base.OnRemove(item);
            
            Destroy(item.agent);
        }

        public int Create(Monster monster, MonsterConfigData configData, MonsterContext context)
        {
            int agent = simulator.CreateAgent(
                monster.transform.position, monster.Radius * context.SizeScale,
                configData.moveSpeed,
                stopDistance + configData.stopDistance
            ).agent;

            Data data = new Data(monster, agent, context.Attack, this);
            Add(data);
            agentToData.Add(agent, data);
           
            return agent;
        }

        public bool TryGet(int agent, out Monster monster)
        {
            if (agentToData.TryGetValue(agent, out Data data))
            {
                monster = data.monster;
                return true;
            }
            else
            {
                monster = null;
                return false;
            }
        }

        public bool TryGetAgent(int agent, out AgentData agentData) => simulator.TryGetAgent(agent, out agentData);

        public bool TryGet(int agent, out Data data)
        {
            return agentToData.TryGetValue(agent, out data);
        }

        public bool IsAlive(int agent)
        {
            return agentToData.ContainsKey(agent);
        }

        public int QueryAgentInRange(Vector3 center, Vector3 size, out AgentData[] agentsData)
        {
            return simulator.QueryAgent(
                new float2(center.x, center.y),
                new float2(size.x, size.y),
                out agentsData
            );
        }

        private void Destroy(int agent)
        {
            if (agentToData.Remove(agent, out var data))
            {
                data.monster.Destroy();
                OnMonsterRemoved?.Invoke();
            }

            simulator.DestroyAgent(agent);
        }

        public class Data : ITickRunner
        {
            public Monster monster;
            public int agent;
            public int attack;

            MonsterTickRunner runner;

            public Data(Monster monster, int agent, int attack, MonsterTickRunner runner)
            {
                this.monster = monster;
                this.agent = agent;
                this.attack = attack;
                this.runner = runner;
            }

            public void Tick(float deltaTime)
            {
                if (runner.simulator.TryGetAgent(agent, out AgentData agentData))
                {
                    if (agentData.isStopped)
                    {
                        if (BaseCore.Instance != null && BaseCore.Instance.IsAlive)
                        {
                            BaseCore.Instance.TakeDamage(attack);
                        }
                        runner.Remove(this);
                        return;
                    }

                    Vector3 position = new Vector3(agentData.position.x, agentData.position.y);
                    monster.SetPosition(position, deltaTime);
                }
            }
        }
    }
}