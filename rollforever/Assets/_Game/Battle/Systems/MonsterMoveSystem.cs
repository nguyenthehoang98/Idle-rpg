using _Game.Battle.Data;
using Geometry;
using GoodCat.EcsLite.Shared;
using Leopotam.EcsLite;
using Unity.Mathematics;

namespace _Game.Battle.Systems
{
    public class MonsterMoveSystem : IEcsInitSystem, IEcsRunSystem, IEcsPostRunSystem
    {
        [EcsInject] private readonly BattleStartupShareData shareData;
        [EcsInject] private readonly BattleStartupRuntimeData runtimeData;

        private const float THREASHOLD_VELOCITYSQ = 0.3f;

        private EcsPool<UnitData> unitPool;
        private EcsFilter ecsFilter;

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            ecsFilter = world.Filter<UnitData>()
                .Inc<MonsterFlag>()
                .Exc<DeadFlag>()
                .End();
            unitPool = world.GetPool<UnitData>();
        }

        public void Run(IEcsSystems systems)
        {
            shareData.Simulator.EnsureCompleted();
            
            shareData.Matrix.ResetTrigger();

            foreach (var e in ecsFilter)
            {
                var unit = unitPool.Get(e);

                if (ShapeInstance.TryGet(unit.shapeId, out var shape))
                {
                    var position = shareData.Simulator.GetAgentPosition(unit.agentId);

                    shareData.Matrix.TriggerPoint(position);

                    if (shape.Type == ShapeType.Circle)
                    {
                        shareData.Matrix.TriggerArea(position, shape.Radius);
                    }
                    else if (shape.Type == ShapeType.Box)
                    {
                        
                    }
                }
            }

            foreach (var e in ecsFilter)
            {
                var unit = unitPool.Get(e);
                var paused = shareData.Simulator.IsAgentPaused(unit.agentId);
                if (paused)
                    continue;

                var goal = shareData.Simulator.GetAgentGoal(unit.agentId);
                var radius = shareData.Simulator.GetAgentRadius(unit.agentId);
                var position = shareData.Simulator.GetAgentPosition(unit.agentId);

                if (ShouldPause(position, goal, radius))
                {
                    Pause(unit.agentId, position);
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

                var velocity = shareData.Simulator.GetAgentVelocity(unit.agentId);
                if (math.lengthsq(velocity) < THREASHOLD_VELOCITYSQ)
                {
                    var position = shareData.Simulator.GetAgentPosition(unit.agentId);
                    Pause(unit.agentId, position);
                }
            }
        }

        void Pause(int agentId, float2 position)
        {
            shareData.Matrix.OccupiedPoint(position);
            
            shareData.Simulator.PauseAgent(agentId, true);
        }

        bool ShouldPause(float2 pos, float2 goal, float radius)
        {
            return math.distancesq(goal, pos) <= shareData.Matrix.radiussq(radius);
        }
    }
}