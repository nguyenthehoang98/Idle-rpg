using System;
using System.Collections.Generic;
using _KITSystem.Utils;
using RVO;
using Sherbert.Framework.Generic;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace _KITSystem.Grid
{
    [Serializable]
    public class AgentGrid : IDisposable
    {
        [TitleGroup("Agent default settings")]
#if UNITY_EDITOR
        [SerializeField] private bool locked;
        [SerializeField] private bool enableGizmos;
#endif
        [SerializeField] private float defaultAgentStopDistance = 3;
        [SerializeField] private float defaultAgentRadius = 0.5f;
        [SerializeField] private float interval = 0.5f;
        [SerializeField, Range(0.1f, 0.9f)] private float multiplierIgnoreCheckDistance = 0.2f;
        [SerializeField, Range(0.1f, 1.0f)] private float deltaDistanceStuck = 0.2f;
        [TitleGroup("Debug")] 
        [SerializeField, DisableIf("@true")] private bool isInitialized;

        private Dictionary<int, AgentData> containers = new Dictionary<int, AgentData>();        
        private List<int> agents = new List<int>();
        private Simulator simulator;
        private IGridManager gridManager;
        private float stopDistanceSq;
        private float ignoreCheckNeighborDistanceSq;
        private float deltaDistanceStuckSq;
        
        protected virtual void OnInitialize()
        { 
        }

        public void Tick(float deltaTime)
        {
            if (locked) return;
            simulator.SetTimeStep(deltaTime);
            simulator.EnsureCompleted();

#if UNITY_EDITOR
            if(enableGizmos) DrawLine(deltaTime);
#endif
            // todo: logic update
            SetPreferredVelocities();
            ReachedGoal();
            simulator.DoStep();
        }
        
        public int QueryAgent(float2 position, float2 size, out AgentData[] agentsData)
        {
            int query = gridManager.Query(position, size, out int[] results);
            agentsData = new AgentData[query];
            int index = 0;
            for (int i = 0; i < query; i++)
            {
                int id = results[i];
                if (containers.TryGetValue(id, out AgentData data))
                {
                    agentsData[index] = data;
                    index++;
                }
            }

            return index;
        }
        
        protected void StopAgent(int agentId)
        {
            simulator.SetAgentMaxSpeed(agentId, 0);
            simulator.SetAgentPrefVelocity(agentId, float2.zero);
        }

        private void DrawLine(float deltaTime)
        {
            void DrawCircle(Vector3 center, float radius, int segments, Color color)
            {
                Vector3 prev = center + Vector3.right * radius;
                for (int i = 1; i <= segments; i++)
                {
                    float t = i / (float)segments;
                    float angle = t * Mathf.PI * 2f;
                    Vector3 next = center + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                    Debug.DrawLine(prev, next, color, deltaTime);
                    prev = next;
                }
            }
            
            foreach (int agent in agents)
            {
                AgentData value = containers[agent];
                float2 position = value.position;
                Color color = value.isStopped ? Color.red : Color.green;
                DrawCircle(new Vector3(position.x, position.y), value.radius, 6, color);
            }
        }

        public void Initialize()
        {
            gridManager = new FixedUniformGrid(1);
            simulator = new Simulator();
            simulator.SetTimeStep(0.25f);
            simulator.SetAgentDefaults(5f, 10, 10f, 10f, defaultAgentRadius, 1f, float2.zero);
            stopDistanceSq = defaultAgentStopDistance * defaultAgentStopDistance;
            float a = 2 * (1 + multiplierIgnoreCheckDistance) * defaultAgentRadius;
            ignoreCheckNeighborDistanceSq = a * a;
            deltaDistanceStuckSq = deltaDistanceStuck * deltaDistanceStuck;
            OnInitialize();
            isInitialized = true;
        }

        private void ReachedGoal()
        {
            foreach (int agent in agents)
            {
                AgentData temp = containers[agent];

                if (temp.isStopped) continue;

                float2 previous = temp.position;
                float2 position = simulator.GetAgentPosition(agent);
                temp.position = position;

                gridManager.Insert(agent, new float2(position.x, position.y));

                if (math.lengthsq(position) < stopDistanceSq)
                {
                    temp.isStopped = true;
                    containers[agent] = temp;
                    StopAgent(agent);
                    continue;
                }

                int query = gridManager.Query(position, new float2(3, 3), out int[] results);
                int frontBlockedCount = 0;

                float2 dirToGoal = MathUtils.NormalizeSafe(-position);
                for (int i1 = 0; i1 < query; i1++)
                {
                    int otherId = results[i1];
                    if (otherId == agent)
                        continue;

                    // chỉ quan tâm frontier
                    if (containers.TryGetValue(otherId, out AgentData other))
                    {
                        if (!other.isStopped) continue;
                    }

                    float2 otherPos = simulator.GetAgentPosition(otherId);
                    float2 toOther = otherPos - position;
                    float lengthsq = math.lengthsq(toOther);
                    if (lengthsq > ignoreCheckNeighborDistanceSq)
                        continue;

                    float dot = math.dot(dirToGoal, MathUtils.NormalizeSafe(toOther));
                    if (dot < 0.5f)
                        continue;

                    frontBlockedCount++;
                }

                float movedDistanceSq = math.distancesq(position, previous);
                bool stuck = movedDistanceSq < deltaDistanceStuckSq;
                bool crowdedFront = frontBlockedCount >= 2;
                if (crowdedFront && stuck)
                {
                    temp.stuckFrames++;
                }
                else
                {
                    temp.stuckFrames = 0;
                }

                if (temp.stuckFrames >= 10)
                {
                    temp.isStopped = true;
                    StopAgent(agent);
                }

                containers[agent] = temp;
            }
        }

        private void SetPreferredVelocities()
        {
            foreach (int agent in agents)
            {
                float2 position = simulator.GetAgentPosition(agent);
                float2 goalVector = MathUtils.NormalizeSafe(-position);
                if (containers[agent].isStopped)
                {
                }
                else
                {
                    simulator.SetAgentPrefVelocity(agent, goalVector);
                }
            }
        }

        public AgentData CreateAgent(int entityId, Vector2 position, float radius)
        {
            simulator.EnsureCompleted();
            int agent = simulator.AddAgent(position);
            simulator.SetAgentRadius(agent, radius);
            agents.Add(agent);
            AgentData data = new AgentData
            {
                entity = entityId,
                agent = agent,
                radius = radius,
                position = new float2(position.x, position.y)
            };
            containers.Add(agent, data);
            gridManager.Insert(agent, position);
            return data;
        }

        public bool TryGetAgent(int agent, out AgentData agentData)
        {
            return containers.TryGetValue(agent, out agentData);
        }
        
        public void DestroyAgent(int agentId)
        {
            if (agents.Remove(agentId))
            {
                simulator.EnsureCompleted();
                simulator.RemoveAgent(agentId);
                gridManager.Remove(agentId);
                containers.Remove(agentId);
            }
        }
        
        public virtual void Dispose()
        {
            simulator?.Dispose();
        }
    }

    [Serializable]
    public struct AgentData
    {
        public int entity;
        public int agent;
        
        public float2 position;
        public float radius;

        public bool isStopped;

        public int stuckFrames;
    }
}