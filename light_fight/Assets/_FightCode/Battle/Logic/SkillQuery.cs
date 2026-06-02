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

        public void RandomTargetPosition(float2 center, float radius, Func<int, bool> funcFilterEntity, out QueryResult result)
        {
            int count = agentGrid.QueryAgent(center, new float2(radius, radius), out AgentData[] agents);

            List<AgentData> temp = new List<AgentData>(count);

            for (int i = 0; i < count; i++)
            {
                AgentData agent = agents[i];

                int entity = agent.entity;

                if (!EntityManager.IsAlive(entity)) continue;

                temp.Add(agent);
            }
            
            CollectionUtils.Shuffle(ref temp);

            result = new QueryResult();

            for (int i = temp.Count - 1; i >= 0; i--)
            {
                AgentData agent = temp[i];

                if (funcFilterEntity(agent.entity))
                {
                    result.IsPrimaryValid = true;
                    result.PrimaryEntity = agent.entity;
                    result.PrimaryPosition = agent.position;
                    return;
                }
                
                result.IsSecondaryValid = true;
                result.SecondaryEntity = agent.entity;
                result.SecondaryPosition = agent.position;
            }
        }

        public void FarthestTargetPosition(float2 center, float radius, Func<int, bool> funcFilterEntity, out QueryResult result)
        {
            float sqrRadius = radius * radius;
            
            int count = agentGrid.QueryAgent(center, new float2(radius, radius), out AgentData[] agents);
            
            float maxDistance = float.MinValue;
            
            result = new QueryResult();

            for (int i = 0; i < count; i++)
            {
                AgentData agent = agents[i];
                
                int entity = agent.entity;

                if (!EntityManager.IsAlive(entity)) continue;
                
                float dsq = math.distancesq(center, agent.position);

                if (dsq > maxDistance && dsq <= sqrRadius)
                {
                    maxDistance = dsq;

                    if (funcFilterEntity(entity))
                    {
                        result.IsPrimaryValid = true;
                        result.PrimaryEntity = agent.entity;
                        result.PrimaryPosition = agent.position;
                        continue;
                    }

                    result.IsSecondaryValid = true;
                    result.SecondaryEntity = agent.entity;
                    result.SecondaryPosition = agent.position;
                }
            }
        }

        public void NearestTargetPosition(float2 center, float radius, Func<int, bool> funcFilterEntity, out QueryResult result)
        {
            float sqrRadius = radius * radius;
            
            int count = agentGrid.QueryAgent(center, new float2(radius, radius), out AgentData[] agents);
            
            float minDistance = float.MaxValue;
            
            result = new QueryResult();

            for (int i = 0; i < count; i++)
            {
                AgentData agent = agents[i];
                
                int entity = agent.entity;

                if (!EntityManager.IsAlive(entity)) continue;

                float dsq = math.distancesq(center, agent.position);

                if (dsq < minDistance && dsq <= sqrRadius)
                {
                    minDistance = dsq;

                    if (funcFilterEntity(entity))
                    {
                        result.IsPrimaryValid = true;
                        result.PrimaryEntity = agent.entity;
                        result.PrimaryPosition = agent.position;
                        continue;
                    }

                    result.IsSecondaryValid = true;
                    result.SecondaryEntity = agent.entity;
                    result.SecondaryPosition = agent.position;
                }
            }
        }

        public List<int> GetEntities(float2 center, Func<int, bool> funcFilterEntity, float radius)
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

                if (funcFilterEntity(entity))
                {
                    float totalRadius = radius + agent.radius;

                    float2 delta = agent.position - center;

                    radiusSq = totalRadius * totalRadius;

                    if (math.lengthsq(delta) <= radiusSq)
                    {
                        results.Add(entity);
                    }
                }
            }
            
            return results;
        }

        public List<int> GetEntities(float2 center, Func<int, bool> funcFilterEntity, float2 size)
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

                if (funcFilterEntity(entity))
                {
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
            }
            
            return results;
        }
    }
}