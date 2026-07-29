using System;
using System.Collections.Generic;
using _GameToolkit.Avoidance;
using _GameToolkit.Updater;
using _TDS.GameConfig;
using UnityEngine;

namespace _TDS.Unit
{
    public class MonsterMoveUpdater : BaseUpdatable
    {
        [SerializeField] private float stopDistanceDefault;
        [SerializeField] private AgentSimulator agentSimulator;
        
        public static MonsterMoveUpdater Instance { get; private set; }
        
        Dictionary<int, Data> agentToData = new Dictionary<int, Data>();
        private Queue<Data> additionQueue = new Queue<Data>();
        private Queue<Data> removeQueue = new Queue<Data>();
        private List<Data> onGoings = new List<Data>();

        private void Awake()
        {
            Instance = this;
            
            agentSimulator.Initialize();
        }

        private void OnDestroy()
        {
            Instance = null;
        }

        public override void Tick(float deltaTime)
        {
            agentSimulator.Tick(deltaTime);
            
            while (additionQueue.Count > 0)
            {
                onGoings.Add(additionQueue.Dequeue());
            }
            
            for (int i = onGoings.Count - 1; i >= 0; i--)
            {
                Data data = onGoings[i];
                
                if (data == null) onGoings.RemoveAt(i);
                else
                {
                    if (agentSimulator.TryGetAgent(data.agent, out var agent))
                    {
                        Vector3 position = new Vector3(agent.position.x, agent.position.y);
                        
                        data.monster.SetPosition(position, deltaTime);
                    }
                }
            }

            while (removeQueue.Count > 0)
            {
                Data data = removeQueue.Dequeue();

                onGoings.Remove(data);
                
                DestroyAgent(data.agent);
            }
        }

        public void CreateAgent(Monster monster, MonsterConfigData configData, MonsterRuntimeData runtimeData)
        {
            int agent = agentSimulator.CreateAgent(
                monster.transform.position, monster.Radius * runtimeData.SizeScale,
                configData.moveSpeed,
                stopDistanceDefault + configData.stopDistance
            ).agent;

            Data data = new Data(monster, agent);
            
            additionQueue.Enqueue(data);
            
            agentToData.Add(agent, data);
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

        public void DestroyAgent(int agent)
        {
            if (agentToData.Remove(agent, out var data))
            {
               data.monster.Destroy();
            }
            
            agentSimulator.DestroyAgent(agent);
        }

        public void DestroyAgent(int agent, ref Action onDestroyedCommand)
        {
            if (agentToData.Remove(agent, out var data))
            {
                onDestroyedCommand += () => data.monster.Destroy();
            }
            
            agentSimulator.DestroyAgent(agent);
        }

        class Data
        {
            public Monster monster;
            public int agent;

            public Data(Monster monster, int agent)
            {
                this.monster = monster;
                this.agent = agent;
            }
        }
    }
}