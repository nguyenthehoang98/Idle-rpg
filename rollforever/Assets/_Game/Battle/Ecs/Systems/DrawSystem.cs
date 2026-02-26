using _Game.Battle.Ecs.Data;
using _Game.Battle.Ecs.Model;
using _Game.Scripts.Model;
using Geometry;
using Geometry.Primary;
using GoodCat.EcsLite.Shared;
using Leopotam.EcsLite;
using RVO;
using Unity.Mathematics;
using UnityEngine;

namespace _Game.Battle.Ecs.Systems
{
    public class DrawSystem : IEcsInitSystem, IEcsPostRunSystem
    {
        [EcsInject] private readonly BattleStartupShareData shareData;
        
        private EcsPool<UnitData> unitPool;
        private EcsPool<HealthData> healthPool;
        private EcsPool<StatData> statPool;
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
            statPool = world.GetPool<StatData>();
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

                statPool.Get(e).TryGetValue(StatType.MaxHealth, out var healthStat);
                float percent = healthPool.Get(e).health / healthStat.Value;
                
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

#if UNITY_EDITOR
            Simulator simulator = shareData.Simulator;
            simulator.EnsureCompleted();
            foreach (var obstacle in shareData.Obstacles)
            {
                var first = simulator.GetFirstObstacleVertexId(obstacle);

                var current = first;

                while (true)
                {
                    var next = simulator.GetNextObstacleVertexId(current);

                    float2 p0 = simulator.GetObstacleVertex(current);
                    float2 p1 = simulator.GetObstacleVertex(next);

                    Debug.DrawLine((Vector2)p0, (Vector2)p1, Color.yellow, shareData.TimeDelta);

                    if (next == first)
                    {
                        break;
                    }

                    current = next;
                }
            }
#endif
            
        }
    }
}