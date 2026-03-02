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
    public class MonsterMoveSystem : IEcsInitSystem, IEcsRunSystem, IEcsPostRunSystem
    {
        [EcsInject] private readonly BattleStartupShareData shareData;
        
        const float THRESHOLD_VELOCITYSQ = 0.1f;
        const float THRESHOLD_DISTANCE = 0.1F;
        const float THRESHOLD_TIME = 2f;

        private EcsPool<MonsterAgentData> agentPool;
        private EcsPool<ShapeData> shapePool;
        private EcsPool<UnitModifierData> modifierPool;
        private EcsPool<MonsterTempData> tempDataPool;
        private EcsFilter filter;

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            filter = world.Filter<MonsterAgentData>()
                .Inc<MonsterTempData>()
                .Inc<UnitModifierData>()
                .Inc<MonsterFlag>()
                .Exc<DeadFlag>()
                .End();
            agentPool = world.GetPool<MonsterAgentData>();
            shapePool = world.GetPool<ShapeData>();
            modifierPool = world.GetPool<UnitModifierData>();
            tempDataPool = world.GetPool<MonsterTempData>();
        }

        public void Run(IEcsSystems systems)
        {
            shareData.Simulator.EnsureCompleted();
            shareData.Matrix.ResetTrigger();

            foreach (var e in filter)
            {
                var agentId = agentPool.Get(e).agentId;
                var shape = shapePool.Get(e);

                var position = shareData.Simulator.GetAgentPosition(agentId);
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
                
#if UNITY_EDITOR
                ref var tempData = ref tempDataPool.Get(e);
                tempData.position = position;
                tempData.velocity = shareData.Simulator.GetAgentVelocity(agentId);
                tempData.prefVelocity = shareData.Simulator.GetAgentPrefVelocity(agentId);
                tempData.newVelocity = shareData.Simulator.GetAgentNewVelocity(agentId);
                tempData.paused = shareData.Simulator.GetAgentPaused(agentId);
#endif
            }

            foreach (var e in filter)
            {
                var agentId = agentPool.Get(e).agentId;
                var paused = shareData.Simulator.IsAgentPaused(agentId);
                if (paused)
                    continue;

                var position = shareData.Simulator.GetAgentPosition(agentId);
                var goal = shareData.Simulator.GetAgentGoal(agentId);
                UnitView.TryUpdatePosition(e, position);
                
                ref var tempData = ref tempDataPool.Get(e);
                if (ShouldPause(modifierPool.Get(e).effect, position, goal, tempData.stopDistance))
                {
                    Pause(e, agentId, position);
                    tempData.isStopped = true;
                    
                    Ray ray = new Ray(position, float2.zero - position);
                    Shape shape = new Shape { type = ShapeType.Box, size = shareData.BoxSize };
                    if (GeometryUtils.Ray(ray, 10, shape, float2.zero, out float length, out float2 point))
                    {
                        // todo: Kiểm tra length
                        Debug.DrawLine((Vector2)position, (Vector2)point, Color.cyan, 10);
                    }
                }
                else
                {
                    if (shareData.Matrix.TryFindCellOutsideAreaFromPivot(goal, float2.zero, 
                            shareData.BoxSize, out var result))
                    {
#if UNITY_EDITOR
                        tempData.goal = result;
#endif
                        shareData.Simulator.SetAgentGoal(agentId, result);
                    }
                    else
                    {
                        shareData.Simulator.SetAgentGoal(agentId);
                        Debug.LogError("out of goal");
                    }

                    tempData.isStopped = false;
                }
            }

            shareData.Simulator.DoStep();

            shareData.Simulator.EnsureCompleted();
        }
        
        public void PostRun(IEcsSystems systems)
        {
            shareData.Simulator.EnsureCompleted();

            foreach (var e in filter)
            {
                var agentId = agentPool.Get(e).agentId;
                var paused = shareData.Simulator.IsAgentPaused(agentId);
                if (paused)
                    continue;

                ref var tempData = ref tempDataPool.Get(e);
                var velocity = shareData.Simulator.GetAgentVelocity(agentId);
                if (math.lengthsq(velocity) < THRESHOLD_VELOCITYSQ)
                {
                    tempData.threasholdVelocityElapsed += shareData.TimeDelta;
                    if (tempData.threasholdVelocityElapsed >= THRESHOLD_TIME)
                    {
                        var position = shareData.Simulator.GetAgentPosition(agentId);
                        Pause(e, agentId, position);
                    }
                }
                else
                {
                    tempData.threasholdVelocityElapsed = 0;
                }
            }
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