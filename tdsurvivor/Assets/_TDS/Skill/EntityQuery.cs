/*
using System;
using System.Collections.Generic;
using _GameToolkit.SkillSystem.Core;
using _TDS.Unit;
using _Toolkit.Avoidance;
using Unity.Mathematics;
using UnityEngine;

namespace _TDS.Skill
{
    /// <summary>
    /// Implement IQuery cho TDSurvivor: query entity qua AgentSimulator spatial grid.
    /// EntityId chính là agent ID từ RVO simulator.
    /// Monster registry: MonsterMoveUpdater.Instance.TryGetMonster(agent, out monster).
    /// </summary>
    [Serializable]
    public class EntityQuery : IQuery
    {
        public void FindTarget(FindTargetType type, int totalQuery, Vector2 center, Vector2 pivot, float radius,
            Func<int, float2, bool> funcFilterEntity, out QueryResult result)
        {
            result = new QueryResult();

            if (MonsterMoveUpdater.Instance == null) return;

            int count = MonsterMoveUpdater.Instance.QueryAgent(center, new Vector2(radius, radius), out AgentData[] agents);

            float sqr = radius * radius;

            List<(float distance, int entity, float2 position)> candidates = new List<(float, int, float2)>();

            for (int i = 0; i < count; i++)
            {
                AgentData data = agents[i];

                switch (type)
                {
                    case FindTargetType.Nearest:
                        float2 centerF = new float2(center.x, center.y);
                        float2 pivotF = new float2(pivot.x, pivot.y);

                        if (math.distancesq(centerF, data.position) >= sqr) continue;

                        if (funcFilterEntity(data.agent, data.position))
                        {
                            float dsq = math.distancesq(pivotF, data.position);
                            candidates.Add((dsq, data.agent, data.position));
                        }

                        break;
                    default:
                        Debug.LogError("Unknown entity type " + type);
                        break;
                }
            }

            candidates.Sort((a, b) => a.distance.CompareTo(b.distance));

            int take = Math.Min(totalQuery, candidates.Count);
            result.Results = new List<QueryEntityData>(take);

            for (int i = 0; i < take; i++)
            {
                Vector2 pos = new Vector2(candidates[i].position.x, candidates[i].position.y);
                result.Results.Add(new QueryEntityData(candidates[i].entity, pos));
            }
        }

        public List<int> GetAllEntities(Vector2 center, Vector2 size, Func<int, bool> funcFilterEntity)
        {
            if (MonsterMoveUpdater.Instance == null) return new List<int>();

            int count = MonsterMoveUpdater.Instance.QueryAgent(center, size, out AgentData[] agents);

            Vector2 half = size * 0.5f;
            float left = center.x - half.x;
            float right = center.x + half.x;
            float top = center.y + half.y;
            float bottom = center.y - half.y;

            List<int> results = new List<int>(count);

            for (int i = 0; i < count; i++)
            {
                AgentData data = agents[i];

                if (!funcFilterEntity(data.agent)) continue;

                float2 p = data.position;

                float r = data.radius;

                float closestX = math.clamp(p.x, left, right);
                float closestY = math.clamp(p.y, bottom, top);

                float dx = p.x - closestX;
                float dy = p.y - closestY;

                if (dx * dx + dy * dy <= r * r)
                {
                    results.Add(data.agent);
                }
            }

            return results;
        }

        public List<int> GetAllEntities(Vector2 center, Vector2 size, Vector2 direction, Func<int, bool> funcFilterEntity)
        {
            if (MonsterMoveUpdater.Instance == null) return new List<int>();

            Vector2 right = new Vector2(direction.y, -direction.x);

            float halfForward = size.x * 0.5f;
            float halfRight = size.y * 0.5f;
            Vector2 queryHalf = new Vector2(
                Mathf.Abs(direction.x) * halfForward + Mathf.Abs(right.x) * halfRight,
                Mathf.Abs(direction.y) * halfForward + Mathf.Abs(right.y) * halfRight
            );

            int count = MonsterMoveUpdater.Instance.QueryAgent(center, queryHalf * 2f, out AgentData[] agents);

            List<int> results = new List<int>(count);

            for (int i = 0; i < count; i++)
            {
                AgentData data = agents[i];

                if (!funcFilterEntity(data.agent)) continue;

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
                    results.Add(data.agent);
                }
            }

            return results;
        }
    }
}
*/
