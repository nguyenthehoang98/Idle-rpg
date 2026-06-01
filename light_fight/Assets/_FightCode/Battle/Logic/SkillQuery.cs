using System;
using System.Collections.Generic;
using _KITSystem.Grid;
using _KITSystem.SkillSystem.Config;
using _KITSystem.SkillSystem.Entity;
using _KITSystem.SkillSystem.Runtime;
using _KITSystem.Utils;
using Unity.Mathematics;

namespace _FightCode.Battle.Logic
{
    public class SkillQuery : IQuery
    {
        private AgentGrid agentGrid;

        public SkillQuery(AgentGrid agentGrid)
        {
            this.agentGrid = agentGrid;
        }

        public bool FindRandomTargetPosition(float2 center, float radius, Func<int, bool> funcFilterEntity, out QueryResult result)
        {
            int count = agentGrid.QueryAgent(center, new float2(radius, radius), out AgentData[] agents);

            List<AgentData> temp = new List<AgentData>(count);

            for (int i = 0; i < count; i++)
            {
                AgentData agent = agents[i];

                int entity = agent.entity;

                if (funcFilterEntity(entity))
                {
                    temp.Add(agent);                    
                }
            }
            

            AgentData agentData = default;

            if (temp.Count > 0)
            {
                int index = RandomUtils.Range(0, temp.Count);
                agentData = temp[index];
            }

            result = new QueryResult
            {
                Entity = agentData.entity,
                Position = agentData.position,
            };
            
            return temp.Count > 0;
        }

        public bool FindFarthestTargetPosition(float2 center, float radius, Func<int, bool> funcFilterEntity, out QueryResult result)
        {
            float sqrRadius = radius * radius;
            
            int count = agentGrid.QueryAgent(center, new float2(radius, radius), out AgentData[] agents);
            
            bool found = false;

            AgentData agentData = default;
            
            float maxDistance = float.MinValue;

            for (int i = 0; i < count; i++)
            {
                AgentData agent = agents[i];
                
                int entity = agent.entity;
                
                if (funcFilterEntity(entity))
                {
                    float dsq = math.distancesq(center, agent.position);

                    if (dsq > maxDistance && dsq <= sqrRadius)
                    {
                        maxDistance = dsq;

                        agentData = agent;
                    
                        found = true;
                    }                  
                }
            }

            result = new QueryResult
            {
                Entity = agentData.entity,
                Position = agentData.position,
            };

            return found;
        }

        public bool FindNearestTargetPosition(float2 center, float radius, Func<int, bool> funcFilterEntity, out QueryResult result)
        {
            float sqrRadius = radius * radius;
            
            int count = agentGrid.QueryAgent(center, new float2(radius, radius), out AgentData[] agents);
            
            bool found = false;

            AgentData agentData = default;
            
            float minDistance = float.MaxValue;

            for (int i = 0; i < count; i++)
            {
                AgentData agent = agents[i];
                
                int entity = agent.entity;

                if (funcFilterEntity(entity))
                {
                    float dsq = math.distancesq(center, agent.position);

                    if (dsq < minDistance && dsq <= sqrRadius)
                    {
                        minDistance = dsq;

                        agentData = agent;
                    
                        found = true;
                    }
                } 
            }

            result = new QueryResult
            {
                Entity = agentData.entity,
                Position = agentData.position,
            };

            return found;
        }

        public List<int> GetEntities(float2 center, float radius)
        {
            float2 signalSize = new float2(radius * 2, radius * 2);
            
            int count = agentGrid.QueryAgent(center, signalSize, out AgentData[] agents);
            
            List<int> results = new List<int>(count);
          
            float radiusSq;

            for (int i = 0; i < count; i++)
            {
                AgentData agent = agents[i];
                
                int entity = agent.entity;

                if (!EntityManager.IsAlive(entity)) continue;

                float totalRadius = radius + agent.radius;

                float2 delta = agent.position - center;

                radiusSq = totalRadius * totalRadius;

                if (math.lengthsq(delta) <= radiusSq)
                {
                    results.Add(entity);
                }
            }
            
            return results;
        }

        public List<int> GetEntities(float2 center, float2 size)
        {
            int count = agentGrid.QueryAgent(center, size, out AgentData[] agents);

            float2 half = size * 0.5f;
            float left = center.x - half.x;
            float right = center.x + half.x;
            float top = center.y + half.y;
            float bottom = center.y - half.y;
            
            List<int> results = new List<int>(count);

            for (int i = 0; i < count; i++)
            {
                AgentData agent = agents[i];

                int entity = agent.entity;

                if (!EntityManager.IsAlive(entity)) continue;

                float2 p = agent.position;
                
                float r = agent.radius;
                
                float closestX = math.clamp(p.x, left, right);
                float closestY = math.clamp(p.y, bottom, top);

                float dx = p.x - closestX;
                float dy = p.y - closestY;
                
                if (dx * dx + dy * dy <= r * r)
                {
                    results.Add(entity);
                }
            }
            
            return results;
        }
    }
}