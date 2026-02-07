using System;
using _Game.Battle.Data;
using Geometry;
using GoodCat.EcsLite.Shared;
using Leopotam.EcsLite;
using Unity.Mathematics;
using UnityEngine;

namespace _Game.Battle.Systems
{
    public class DrawSystem : IEcsInitSystem, IEcsPostRunSystem
    {
        [EcsInject] private readonly BattleStartupShareData shareData;
        
        private EcsPool<UnitData> unitPool;
        private EcsFilter ecsFilter;
        
        public void Init(IEcsSystems systems)
        {
            EcsWorld world = systems.GetWorld();
            ecsFilter = world.Filter<UnitData>()
                .End();
            unitPool = world.GetPool<UnitData>();
        }

        public void PostRun(IEcsSystems systems)
        {
#if UNITY_EDITOR
            foreach (var e in ecsFilter)
            {
                var unit = unitPool.Get(e);
                    
                float2 position = shareData.Simulator.GetAgentPosition(unit.agentId);
                float radius = shareData.Simulator.GetAgentRadius(unit.agentId);
                float2 velocity = shareData.Simulator.GetAgentVelocity(unit.agentId);
                float neighborDist = shareData.Simulator.GetAgentNeighborDist(unit.agentId);
                ShapeInstance.TryGet(unit.shapeId, out var shape);
                Circle2D(position, radius, unit.color, 12, shareData.TimeDelta);
                Circle2D(position, neighborDist, new Color(0, 1, 1, 0.05f), 12, shareData.TimeDelta);
                Debug.DrawRay((Vector2)position, ((Vector2)velocity).normalized * radius);
            }
#endif
        }
        
        static void Circle2D(float2 center, float radius, Color col, int samples, float deltaTime)
        {
            float2 from, to;

            float angleIncrease = (float)(Math.PI * 2) / samples;
            from = to = new float2(center.x + radius * Mathf.Cos(0.0f), center.y + radius * Mathf.Sin(0.0f));

            for (int i = 0; i < samples; i++)
            {
                float rad = angleIncrease * (i + 1);

                to = new float2(center.x + radius * Mathf.Cos(rad), center.y + radius * Mathf.Sin(rad));

                Line(from, to, col, deltaTime);

                from = to;
            }
        }

        static void Box2D(float2 center, float2 size, Color color, float deltaTime)
        {
            float2 half = size * 0.5f;

            Vector3 p1 = new Vector3(center.x - half.x, center.y - half.y, 0);
            Vector3 p2 = new Vector3(center.x + half.x, center.y - half.y, 0);
            Vector3 p3 = new Vector3(center.x + half.x, center.y + half.y, 0);
            Vector3 p4 = new Vector3(center.x - half.x, center.y + half.y, 0);

            Debug.DrawLine(p1, p2, color, deltaTime);
            Debug.DrawLine(p2, p3, color, deltaTime);
            Debug.DrawLine(p3, p4, color, deltaTime);
            Debug.DrawLine(p4, p1, color, deltaTime);
        }

        static void Box2dSelected(float2 center, float2 size, Color color, float deltaTime)
        {
            float2 half = size * 0.5f;

            Vector3 p1 = new Vector3(center.x - half.x, center.y - half.y, 0);
            Vector3 p2 = new Vector3(center.x + half.x, center.y - half.y, 0);
            Vector3 p3 = new Vector3(center.x + half.x, center.y + half.y, 0);
            Vector3 p4 = new Vector3(center.x - half.x, center.y + half.y, 0);

            Debug.DrawLine(p1, p3, color, deltaTime);
            Debug.DrawLine(p2, p4, color, deltaTime);
        }
        
        static void Line(float2 from, float2 to, Color col, float deltaTime)
        {
            Debug.DrawLine((Vector2) from, (Vector2) to, col, deltaTime);
        }
    }
}