using System;
using _Game.Battle.AbilitySystem;
using _Game.Battle.Ecs.Data;
using _Game.Battle.Ecs.Model;
using _Game.Battle.Ecs.View;
using Geometry;
using Geometry.Primary;
using GoodCat.EcsLite.Shared;
using Leopotam.EcsLite;
using Unity.Mathematics;
using UnityEngine;
using Ray = Geometry.Primary.Ray;

namespace _Game.Battle.Ecs.Systems
{
    public class MonsterMoveSystem : IEcsInitSystem, IEcsRunSystem
    {
        [EcsInject] private readonly BattleStartupShareData shareData;
        
        const float THRESHOLD_DISTANCE = 0.1F;

        private EcsPool<UnitData> unitPool;
        private EcsPool<ShapeData> shapePool;
        private EcsPool<UnitModifierData> modifierPool;
        private EcsPool<UnitPosTempData> unitPosTempPool;
        private EcsFilter filter;

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            filter = world.Filter<UnitData>()
                .Inc<UnitPosTempData>()
                .Inc<UnitModifierData>()
                .Inc<MonsterFlag>()
                .Exc<DeadFlag>()
                .End();
            unitPool = world.GetPool<UnitData>();
            shapePool = world.GetPool<ShapeData>();
            modifierPool = world.GetPool<UnitModifierData>();
            unitPosTempPool = world.GetPool<UnitPosTempData>();
        }

        public void Run(IEcsSystems systems)
        {
            shareData.Simulator.EnsureCompleted();
            shareData.Matrix.ResetTrigger();

            foreach (var e in filter)
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
#if DEVELOP_MODE
                    throw new Exception($"Shape {shape.Value.type} chưa được xác định");        
#endif                
                }
            }

            foreach (var e in filter)
            {
                var unit = unitPool.Get(e);
                var paused = shareData.Simulator.IsAgentPaused(unit.agentId);
                if (paused)
                    continue;

                var position = shareData.Simulator.GetAgentPosition(unit.agentId);
                var goal = shareData.Simulator.GetAgentGoal(unit.agentId);
                UnitView.TryUpdatePosition(e, position);
                
                ref var unitPosTemp = ref unitPosTempPool.Get(e);
                if (ShouldPause(modifierPool.Get(e).effect, position, goal, unitPosTemp.stopDistance))
                {
                    Pause(e, unit.agentId, position);
                    unitPosTemp.isStopped = true;
                }
                else
                {
                    if (shareData.Matrix.TryFindCellOutsideAreaFromPivot(goal, float2.zero, shareData.BoxSize, out var result))
                    {
                        shareData.Simulator.SetAgentGoal(unit.agentId, result);
                    }
                    else
                    {
                        shareData.Simulator.SetAgentGoal(unit.agentId);
                    }
                    unitPosTemp.isStopped = false;
                }
            }

            shareData.Simulator.DoStep();

            shareData.Simulator.EnsureCompleted();
        }

        void Pause(int entity, int agentId, float2 position)
        {
            shareData.Matrix.OccupiedPoint(entity, position);
            shareData.Simulator.PauseAgent(agentId, true);
        }

        bool ShouldPause(StatusEffect effect, float2 point, float2 goal, float2 boxSize)
        {
            if (effect.Has(StatusEffect.Stun)) return true;
            if (effect.Has(StatusEffect.KnockBack)) return true;
            Ray ray = new Ray(point, point - goal);
            Shape shape = new Shape { type = ShapeType.Box, size = boxSize };
            if (GeometryUtils.Ray(ray, 99, shape, float2.zero, out var hitLength, out var hitPoint))
            {
                // Khoảng cách từ điểm -> center có 
                float d1 = math.distance(point, goal);
                return hitLength - d1 < THRESHOLD_DISTANCE;
            }

            return false;
        }
    }
}