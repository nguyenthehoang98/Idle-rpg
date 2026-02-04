using System;
using _Game.Battle.Data;
using GoodCat.EcsLite.Shared;
using Leopotam.EcsLite;
using Unity.Mathematics;
using UnityEngine;

namespace _Game.Battle.Systems
{
    public class MonsterMoveSystem : IEcsInitSystem, IEcsRunSystem
    {
        [EcsInject] private readonly BattleStartupShareData shareData;
        [EcsInject] private readonly BattleStartupRuntimeData runtimeData;
        
        private EcsPool<Unit> unitPool;
        private EcsFilter ecsFilter;
        
        public void Init(IEcsSystems systems)
        {
            EcsWorld world = systems.GetWorld();
            ecsFilter = world.Filter<Unit>()
                .End();
            unitPool = world.GetPool<Unit>();
        }

        public void Run(IEcsSystems systems)
        {
            shareData.Simulator.EnsureCompleted();
            foreach (var e in ecsFilter)
            {
                var unit = unitPool.Get(e);
                shareData.Simulator.SyncAgentVelocity(unit.agentId);
#if UNITY_EDITOR
                float2 position = shareData.Simulator.GetAgentPosition(unit.agentId);
                float radius = shareData.Simulator.GetAgentRadius(unit.agentId);
                float2 velocity = shareData.Simulator.GetAgentVelocity(unit.agentId);
                float neighborDist = shareData.Simulator.GetAgentNeighborDist(unit.agentId);
                Circle2D(position, radius, Color.gray, 12, shareData.TimeDelta);
                Circle2D(position, neighborDist, Color.cyan, 12, shareData.TimeDelta);
                Debug.DrawRay((Vector2)position, ((Vector2)velocity).normalized * radius);
#endif
            }
            
            shareData.Simulator.DoStep();
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
        
        static void Line(float2 from, float2 to, Color col, float deltaTime)
        {
            Debug.DrawLine((Vector2) from, (Vector2) to, col, deltaTime);
        }
    }
}