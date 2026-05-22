using System.Collections.Generic;
using _KITSystem.Grid;
using _KITSystem.SkillSystem.Entity;
using _KITSystem.SkillSystem.Runtime;
using Unity.Mathematics;
using UnityEngine;

namespace _Games.Battle
{
    public class SkillQuery : IQuery
    {
        private AgentGrid agentGrid;

        public SkillQuery(AgentGrid agentGrid)
        {
            this.agentGrid = agentGrid;
        }

        public List<int> GetUnits(float2 center, float radius)
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
            
            if(results.Count > 0) Debug.Log(string.Join(", ", results));
            
            return results;
        }

        public List<int> GetUnits(float2 center, float2 size)
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
                    results.Add(agent.agent);
                }
            }
            
            if(results.Count > 0) Debug.Log(string.Join(", ", results));
            
            return results;
        }
    }
}