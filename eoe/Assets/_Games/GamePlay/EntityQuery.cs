using System;
using System.Collections.Generic;
using _KITSystem.Entity;
using _KITSystem.Grid;
using _KITSystem.SkillSystem.Core;
using Unity.Mathematics;
using UnityEngine;

namespace _Games.GamePlay
{
    [Serializable]
    public class EntityQuery : IQuery
    {
        public void FindTarget(FindTargetType type, Vector2 center, float radius, Func<int, bool> funcFilterEntity,
            out QueryResult result)
        {
            float sqrRadius = radius * radius;

            int count = AgentTickable.Query(center, new float2(radius, radius), out AgentData[] agents);

            float maxDistance = float.MinValue;
            float minDistance = float.MaxValue;

            result = new QueryResult();

            for (int i = 0; i < count; i++)
            {
                AgentData data = agents[i];

                int entity = AgentTickable.GetEntity(data.agent);

                if (!EntityManager.IsEntityAlive(entity)) continue;

                switch (type)
                {
                    case FindTargetType.Farthest:
                    case FindTargetType.Nearest:
                        float dsq = math.distancesq(center, data.position);

                        if (dsq > sqrRadius) continue;

                        if (type == FindTargetType.Farthest)
                        {
                            if (dsq < maxDistance) continue;

                            maxDistance = dsq;
                        }
                        else
                        {
                            if (dsq > minDistance) continue;

                            minDistance = dsq;
                        }

                        if (funcFilterEntity(entity))
                        {
                            result.Primary = new QueryEntityData(entity, data.position);
                            continue;
                        }

                        result.Secondary = new QueryEntityData(entity, data.position);
                        break;
                    case FindTargetType.Filter:
                        break;
                }
            }
        }

        public List<int> GetAllEntities(Vector2 center, Vector2 size, Func<int, bool> funcFilterEntity)
        {
            int count = AgentTickable.Query(center, size, out AgentData[] agents);

            float2 half = size * 0.5f;
            float left = center.x - half.x;
            float right = center.x + half.x;
            float top = center.y + half.y;
            float bottom = center.y - half.y;
            
            List<int> results = new List<int>(count);

            for (int i = 0; i < count; i++)
            {
                AgentData data = agents[i];

                int entity = AgentTickable.GetEntity(data.agent);

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
    }
}