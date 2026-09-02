using System;
using System.Collections.Generic;
using _Game.GamePlay.Manager;
using _Game.GamePlay.Utils;
using _KITSystem.Entity;
using _KITSystem.Grid;
using _KITSystem.SkillSystem.Core;
using Unity.Mathematics;
using UnityEngine;

namespace _Game.GamePlay.Model
{
    [Serializable]
    public class EntityQuery : IQuery
    {
        public void FindTarget(FindTargetType type, Vector2 center, Vector2 pivot, float radius, Func<int, float2, bool> funcFilterEntity,
            out QueryResult result)
        {
            int count = AgentManager.Query_Agent(center, new float2(radius, radius), out AgentData[] agents);
            
            result = new QueryResult();
            
            float sqr = radius * radius;
            
            float minDistance = float.MaxValue;

            for (int i = 0; i < count; i++)
            {
                AgentData data = agents[i];

                if (!MonsterEntityManager.TryGetEntity(data.agent, out int entity)) continue;

                if (!EntityManager.IsEntityAlive(entity)) continue;

                switch (type)
                {
                    case FindTargetType.Nearest:
                        
                        if (math.distancesq(center, data.position) >= sqr) continue;
                        
                        float dsq = math.distancesq(pivot, data.position);

                        if (dsq < minDistance)
                        {
                            minDistance = dsq;

                            if (funcFilterEntity(entity, data.position))
                            {
                                result.Primary = new QueryEntityData(entity, data.position);
                                continue;
                            }

                            result.Secondary = new QueryEntityData(entity, data.position);
                        }
                        
                        break;
                    default: Debug.LogError("Unknown entity type " + type);
                        break;
                }
            }
        }

        public List<int> GetAllEntities(Vector2 center, Vector2 size, Func<int, bool> funcFilterEntity)
        {
            int count = AgentManager.Query_Agent(center, size, out AgentData[] agents);

            float2 half = size * 0.5f;
            float left = center.x - half.x;
            float right = center.x + half.x;
            float top = center.y + half.y;
            float bottom = center.y - half.y;
            
            List<int> results = new List<int>(count);

            for (int i = 0; i < count; i++)
            {
                AgentData data = agents[i];

                if (!MonsterEntityManager.TryGetEntity(data.agent, out int entity)) continue;

                if (!EntityManager.IsEntityAlive(entity)) continue;

                if (funcFilterEntity(entity))
                {
                    float2 p = data.position;

                    float r = data.radius;

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

        public List<int> GetAllEntities(Vector2 center, Vector2 size, Vector2 direction, Func<int, bool> funcFilterEntity)
        {
            Vector2 right = new Vector2(direction.y, -direction.x);

            float halfForward = size.x * 0.5f;
            float halfRight = size.y * 0.5f;
            Vector2 queryHalf = new Vector2(
                Mathf.Abs(direction.x) * halfForward + Mathf.Abs(right.x) * halfRight,
                Mathf.Abs(direction.y) * halfForward + Mathf.Abs(right.y) * halfRight
            );
          
            int count = AgentManager.Query_Agent(center, queryHalf * 2f, out AgentData[] agents);

            List<int> results = new List<int>(count);

            for (int i = 0; i < count; i++)
            {
                AgentData data = agents[i];

                if (!MonsterEntityManager.TryGetEntity(data.agent, out int entity))
                    continue;

                if (!EntityManager.IsEntityAlive(entity))
                    continue;

                if (!funcFilterEntity(entity))
                    continue;

                Vector2 delta = (Vector2)data.position - center;

                float localForward = Vector2.Dot(delta, direction);
                float localRight = Vector2.Dot(delta, right);

                float closestForward = Mathf.Clamp(localForward, -halfForward, halfForward);
                float closestRight = Mathf.Clamp(localRight, -halfRight, halfRight);

                float dx = localForward - closestForward;
                float dy = localRight - closestRight;

                bool hit = dx * dx + dy * dy <= data.radius * data.radius;

                if (hit)
                {
                    results.Add(entity);
                }
            }

            return results;
        }
    }
}