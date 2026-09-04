using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using RVO;
using Unity.Mathematics;
using UnityEngine;

namespace _GameToolkit.Avoidance
{
    [Serializable]
    public class AgentSimulator : IDisposable
    {
        [Header("Agent default settings")]
#if UNITY_EDITOR
        public bool enableGizmos;
#endif
        [SerializeField] private float2 destination;

        /// <summary>Điểm đến các agent hướng tới (mặc định hero). Gán động mỗi tick.</summary>
        public Vector2 Destination
        {
            get => new Vector2(destination.x, destination.y);
            set => destination = new float2(value.x, value.y);
        }

        private Dictionary<int, AgentData> containers = new Dictionary<int, AgentData>();        
        private List<int> agents = new List<int>();
        private Simulator simulator;
        
        public virtual void Tick(float deltaTime)
        {
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
        
        protected void StopAgent(int agent)
        {
            simulator.SetAgentMaxSpeed(agent, 0);
            simulator.SetAgentPrefVelocity(agent, float2.zero);
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
            simulator = new Simulator();
            simulator.SetTimeStep(0.25f);
            simulator.SetAgentDefaults(5f, 10, 10f, 10f, 1, 1f, float2.zero);
        }

        private void ReachedGoal()
        {
            foreach (int agent in agents)
            {
                AgentData temp = containers[agent];

                if (temp.isStopped) continue;

                float2 position = simulator.GetAgentPosition(agent);
             
                temp.position = position;

                if (math.lengthsq(position) < temp.stopDistanceSq)
                {
                    temp.isStopped = true;
                    containers[agent] = temp;
                    StopAgent(agent);
                    continue;
                }

                containers[agent] = temp;
            }
        }

        private void SetPreferredVelocities()
        {
            foreach (int agent in agents)
            {
                float2 position = simulator.GetAgentPosition(agent);
                float2 goalVector = NormalizeSafe(destination - position);
                if (containers[agent].isStopped)
                {
                }
                else
                {
                    simulator.SetAgentPrefVelocity(agent, goalVector);
                }
            }
        }

        public AgentData CreateAgent(Vector2 position, float radius, float speed, float stopDistance)
        {
            simulator.EnsureCompleted();
            
            int agent = simulator.AddAgent(position);
            
            simulator.SetAgentRadius(agent, radius);
            simulator.SetAgentMaxSpeed(agent, speed);
            
            agents.Add(agent);
            
            AgentData data = new AgentData
            {
                agent = agent,
                radius = radius,
                stopDistanceSq = stopDistance * stopDistance,
                position = new float2(position.x, position.y)
            };
            
            containers.Add(agent, data);
            
            return data;
        }

        public bool TryGetAgent(int agent, out AgentData agentData)
        {
            return containers.TryGetValue(agent, out agentData);
        }
        
        public void SetAgentMaxSpeed(int agent, float speed)
        {
            simulator.SetAgentMaxSpeed(agent, Mathf.Max(0f, speed));
        }

        public void DestroyAgent(int agent)
        {
            if (agents.Remove(agent))
            {
                simulator.EnsureCompleted();
                simulator.RemoveAgent(agent);
                containers.Remove(agent);
            }
        }
        
        public virtual void Dispose()
        {
            simulator?.Dispose();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static Vector2 NormalizeSafe(Vector2 v)
        {
            float magSq = v.sqrMagnitude;
            if (magSq < 1e-6f) return Vector2.zero;
            return v * (1.0f / Mathf.Sqrt(magSq));
        }
    }
    
    [Serializable]
    public struct AgentData
    {
        public int agent;
        
        public float2 position;
        public float radius;
        public float stopDistanceSq;

        public bool isStopped;
    }
}