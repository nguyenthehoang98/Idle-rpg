using System;
using _Game.Battle.Data;
using Geometry.Primary;
using GoodCat.EcsLite.Shared;
using Leopotam.EcsLite;
using Unity.Mathematics;

namespace _Game.Battle.Systems
{
    public class MonsterMoveSystem : IEcsInitSystem, IEcsRunSystem, IEcsPostRunSystem
    {
        [EcsInject] private readonly BattleStartupShareData shareData;
        [EcsInject] private readonly BattleStartupRuntimeData runtimeData;

        private const float THREASHOLD_VELOCITYSQ = 2.0f;
        private const float THREASHOLD_TIME = 1f;

        private EcsPool<UnitData> unitPool;
        private EcsPool<ShapeData> shapePool;
        private EcsPool<UnitPosTempData> unitPosTempPool;
        private EcsFilter ecsFilter;

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            ecsFilter = world.Filter<UnitData>()
                .Inc<UnitPosTempData>()
                .Inc<MonsterFlag>()
                .Exc<DeadFlag>()
                .End();
            unitPool = world.GetPool<UnitData>();
            shapePool = world.GetPool<ShapeData>();
            unitPosTempPool = world.GetPool<UnitPosTempData>();
        }

        public void Run(IEcsSystems systems)
        {
            shareData.Simulator.EnsureCompleted();

            shareData.Matrix.ResetTrigger();

            foreach (var e in ecsFilter)
            {
                var unit = unitPool.Get(e);
                var shape = shapePool.Get(e);

                var position = shareData.Simulator.GetAgentPosition(unit.agentId);
                shareData.Matrix.TriggerPoint(e, position);

                if (shape.Value.type == ShapeType.Circle)
                {
                    shareData.Matrix.TriggerArea(e, position, shape.Value.radius);   
                }
                else if (shape.Value.type == ShapeType.Box)
                {
                    shareData.Matrix.TriggerArea(e, position, shape.Value.size);   
                }
                else
                {
                    throw new Exception("Shape chưa được xác định: " + shape.Value.type);                    
                }
            }

            foreach (var e in ecsFilter)
            {
                var unit = unitPool.Get(e);
                var paused = shareData.Simulator.IsAgentPaused(unit.agentId);
                if (paused)
                    continue;

                var position = shareData.Simulator.GetAgentPosition(unit.agentId);
                var goal = shareData.Simulator.GetAgentGoal(unit.agentId);
                var radius = shareData.Simulator.GetAgentRadius(unit.agentId);

                if (ShouldPause(position, goal, radius))
                {
                    Pause(e, unit.agentId, position);
                }
                else
                {
                    if (shareData.Matrix.TryFindCellExpandFromCenter(position, goal, out var result))
                    {
                        shareData.Simulator.SetAgentGoal(unit.agentId, result);
                    }
                    else
                    {
                        shareData.Simulator.SetAgentGoal(unit.agentId);
                    }
                }
            }

            shareData.Simulator.DoStep();

            shareData.Simulator.EnsureCompleted();
        }

        public void PostRun(IEcsSystems systems)
        {
            shareData.Simulator.EnsureCompleted();

            foreach (var e in ecsFilter)
            {
                var unit = unitPool.Get(e);

                var paused = shareData.Simulator.IsAgentPaused(unit.agentId);
                if (paused)
                    continue;

                ref var unitPosTemp = ref unitPosTempPool.Get(e);
                
                var velocity = shareData.Simulator.GetAgentVelocity(unit.agentId);
                if (math.lengthsq(velocity) < THREASHOLD_VELOCITYSQ)
                {
                    unitPosTemp.threasholdVelocityElapsed += shareData.TimeDelta;
                    if (unitPosTemp.threasholdVelocityElapsed >= THREASHOLD_TIME)
                    {
                        var position = shareData.Simulator.GetAgentPosition(unit.agentId);
                        Pause(e, unit.agentId, position);                        
                    }
                }
                else
                {
                    unitPosTemp.threasholdVelocityElapsed = 0;
                }
            }
        }

        void Pause(int entity, int agentId, float2 position)
        {
            shareData.Matrix.OccupiedPoint(entity, position);

            shareData.Simulator.PauseAgent(agentId, true);
        }

        bool ShouldPause(float2 pos, float2 goal, float radius)
        {
            return math.distancesq(goal, pos) <= shareData.Matrix.radiussq(radius);
        }
    }
}