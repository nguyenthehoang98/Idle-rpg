using System;
using _Game.Battle.Data;
using Geometry;
using Geometry.Primary;
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
        private EcsPool<HealthData> healthPool;
        private EcsPool<ShapeData> shapePool;
        private EcsFilter ecsFilter;
        
        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            ecsFilter = world.Filter<UnitData>()
                .Inc<ShapeData>()
                .Inc<HealthData>()
                .End();
            healthPool = world.GetPool<HealthData>();
            unitPool = world.GetPool<UnitData>();
            shapePool = world.GetPool<ShapeData>();
        }

        public void PostRun(IEcsSystems systems)
        {
#if UNITY_EDITOR
            foreach (var e in ecsFilter)
            {
                var unit = unitPool.Get(e);
                var position = shareData.Simulator.GetAgentPosition(unit.agentId);
                var radius = shareData.Simulator.GetAgentRadius(unit.agentId);
                var velocity = shareData.Simulator.GetAgentVelocity(unit.agentId);
                var neighborDist = shareData.Simulator.GetAgentNeighborDist(unit.agentId);

                var health = healthPool.Get(e);
                float percent = health.health / (float) health.maxHealth;
                
                GeometryGizmos.DrawCircle(
                    new Circle(position, radius), unit.color, shareData.TimeDelta, percent, 12
                );

                var paused = shareData.Simulator.IsAgentPaused(unit.agentId);
                if (paused)
                    continue;

                GeometryGizmos.DrawCircle(
                    new Circle(position, neighborDist), new Color(0, 1, 1, 0.05f), shareData.TimeDelta, percent, 12
                );
                Debug.DrawRay((Vector2) position, ((Vector2) velocity).normalized * radius);
            }
#endif
        }
    }
}