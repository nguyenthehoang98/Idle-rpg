using _Game.Battle.Data;
using Leopotam.EcsLite;
using Unity.Mathematics;

namespace _Game.Battle.Systems
{
    public class CleanupSystem : IEcsInitSystem, IEcsPostRunSystem
    {
        private BattleStartupShareData shareData;
        private EcsPool<UnitPos> unitPosPool;
        private EcsPool<Unit> unitPool;
        private EcsFilter ecsFilter;
        private EcsWorld world;
        
        public void Init(IEcsSystems systems)
        {
            world = systems.GetWorld();
            ecsFilter = world.Filter<UnitPos>()
                .Inc<Unit>()
                .End();
            unitPool = world.GetPool<Unit>();
            unitPosPool = world.GetPool<UnitPos>();
            shareData = systems.GetShared<BattleStartupShareData>();
        }

        public void PostRun(IEcsSystems systems)
        {
            foreach (var e in ecsFilter)
            {
                var unitPos = unitPosPool.Get(e);
                if (math.distance(unitPos.goal, unitPos.curPos) < 1)
                {
                    DisposeUnit(e, unitPool.Get(e));
                }
            }
        }

        void DisposeUnit(int e, Unit unit)
        {
            world.DelEntity(e);
            shareData.Simulator.RemoveAgent(unit.agentId);
            shareData.Grid.Remove(unit.cellId);
            ShapeInstance.Remove(unit.shapeId);
        }
    }
}