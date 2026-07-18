using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using _Game.Configs;
using _Game.GamePlay.Data;
using _Game.GamePlay.Model;
using _KITSystem.Grid;
using _KITSystem.Resource;
using _KITSystem.Schedule;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

namespace _Game.GamePlay.Manager
{
    [Serializable]
    public sealed class AgentManager : AgentSimulator, ITickable
    {
        [SerializeField] private float stopDistance = 1.6f;
        
        static AgentManager instance;

        private Dictionary<int, Temp> container = new Dictionary<int, Temp>();
        private List<Temp> list = new List<Temp>();
        private Queue<Temp> additional = new Queue<Temp>();
        private Queue<Temp> remove = new Queue<Temp>();

        public new Task Initialize()
        {
            base.Initialize();
            
            instance = this;
            return Task.CompletedTask;
        }

        public override void Tick(float deltaTime)
        {
            base.Tick(deltaTime);

            while (additional.Count > 0) list.Add(additional.Dequeue());

            for (int i = list.Count - 1; i >= 0; i--)
            {
                Temp temp = list[i];
                
                if (temp == null) list.RemoveAt(i);
                else
                {
                    if (TryGetAgent(temp.Agent, out var agent))
                    {
                        Vector3 position = new Vector3(agent.position.x, agent.position.y);
                        
                        temp.Monster.SetPosition(position, deltaTime);
                    }
                }
            }
            
            while (remove.Count > 0)
            {
                Temp temp = remove.Dequeue();
                DestroyAgent(temp.Agent);
                Destroy_Agent(temp.Agent);
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            
            instance = null;
        }

        public static void Create_Agent(Monster monster, MonsterRuntimeData runtimeData, MonsterData monsterData)
        {
#if !UNITY_EDITOR
            if (instance == null) return;
#endif
            instance.CreateAgent_Private(monster, runtimeData, monsterData);
        }

        public static int Query_Agent(float2 position, float2 size, out AgentData[] agentsData)
        {
#if !UNITY_EDITOR
            if (instance == null)
            {
                agentsData = null;
                return 0;
            }       
#endif
            return instance.QueryAgent(position, size, out agentsData);
        }

        public static bool TryGet_AgentPosition(int agent, out Vector3 position)
        {
#if !UNITY_EDITOR
            if (instance == null)
            {
                position = Vector3.zero;
                return false;
            }
#endif
            if (instance.TryGetAgent(agent, out AgentData agentData))
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
        
        public static bool TryGet_Monster(int agent, out Monster monster)
        {
#if !UNITY_EDITOR
            if (instance == null)
            {
                monster = null;
                return false;
            }
#endif
            if (instance.container.TryGetValue(agent, out Temp temp))
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

        public static void Destroy_Agent(int agent, ref Action onDestroyMonsterCommand)
        {
#if !UNITY_EDITOR
            if (instance == null) return;       
#endif
            instance.DestroyAgent_Private(agent, ref onDestroyMonsterCommand);
        }

        public static void Destroy_Agent(int agent)
        {
#if !UNITY_EDITOR
            if (instance == null) return;       
#endif
            instance.DestroyAgent_Private(agent);
        }

        private void CreateAgent_Private(Monster monster, MonsterRuntimeData runtimeData, MonsterData monsterData)
        {
            int agent = CreateAgent(
                monster.transform.position, monster.Radius, monsterData.speed,
                stopDistance + monsterData.stopDistance
            ).agent;
            
            monster.Initialize(monsterData, runtimeData);
            
            Temp temp = new Temp(monster, agent);
            additional.Enqueue(temp);
            container.Add(agent, temp);
            
            MonsterEntityManager.CreateEntity(agent, runtimeData, monsterData);
        }

        private void DestroyAgent_Private(int agent)
        {
            if (container.Remove(agent, out Temp temp))
            {
                Monster m = temp.Monster;
                m.Destroy();
                remove.Enqueue(temp);
            }
        }

        private void DestroyAgent_Private(int agent, ref Action onDestroyMonsterCommand)
        {
            if (container.Remove(agent, out Temp temp))
            {
                Monster m = temp.Monster;
                onDestroyMonsterCommand += () => { m.Destroy(); };
                
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