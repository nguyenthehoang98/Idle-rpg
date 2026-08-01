using System;
using System.Collections.Generic;
using _TDS.Config;
using _Toolkit.Avoidance;
using _Toolkit.Updater;
using Unity.Mathematics;
using UnityEngine;

namespace _TDS.GameplayScene.Unit
{
    public class MonsterMovementTickRunner : BaseTickRunner<MonsterMovementTickRunner.Data>
    {
        [SerializeField] float stopDistance;
        [SerializeField] AgentSimulator simulator;

        public static MonsterMovementTickRunner Instance { get; private set; }
        
        Dictionary<int, Data> agentToData = new Dictionary<int, Data>();

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
            
            DestroyMoveAgent(item.agent);
        }

        public int CreateMoveAgent(Monster monster, MonsterConfigData configData, MonsterRuntimeData runtimeData)
        {
            int agent = simulator.CreateAgent(
                monster.transform.position, monster.Radius * runtimeData.SizeScale,
                configData.moveSpeed,
                stopDistance + configData.stopDistance
            ).agent;

            Data data = new Data(monster, agent, this);
            Add(data);
            agentToData.Add(agent, data);
           
            return agent;
        }

        public bool TryGetMonster(int agent, out Monster monster)
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

        public int QueryAgentInRange(Vector3 center, Vector3 size, out AgentData[] agentsData)
        {
            return simulator.QueryAgent(
                new float2(center.x, center.y),
                new float2(size.x, size.y),
                out agentsData
            );
        }
        
        public void DestroyMoveAgent(int agent, ref Action onDestroyedCommand)
        {
            if (agentToData.Remove(agent, out var data))
            {
                onDestroyedCommand += () => data.monster.Destroy();
            }
            
            simulator.DestroyAgent(agent);
        }

        public void DestroyMoveAgent(int agent)
        {
            if (agentToData.Remove(agent, out var data))
            {
                data.monster.Destroy();
            }

            simulator.DestroyAgent(agent);
        }

        public class Data : ITickRunner
        {
            public Monster monster;
            public int agent;

            MonsterMovementTickRunner runner;

            public Data(Monster monster, int agent, MonsterMovementTickRunner runner)
            {
                this.monster = monster;
                this.agent = agent;
                this.runner = runner;
            }

            public void Tick(float deltaTime)
            {
                if (runner.simulator.TryGetAgent(agent, out AgentData agentData))
                {
                    Vector3 position = new Vector3(agentData.position.x, agentData.position.y);
                    monster.SetPosition(position, deltaTime);
                }
            }
        }
    }
}