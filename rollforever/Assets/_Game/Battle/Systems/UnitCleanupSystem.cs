using _Game.Battle.Data;
using Geometry;
using GoodCat.EcsLite.Shared;
using Leopotam.EcsLite;

namespace _Game.Battle.Systems
{
    public class UnitCleanupSystem : IEcsInitSystem, IEcsPostRunSystem
    {
        [EcsInject] private readonly BattleStartupShareData shareData;
        [EcsInject] private readonly BattleStartupRuntimeData runtimeData;

        private EcsPool<UnitData> unitPool;
        private EcsFilter aliveFilter;
        private EcsFilter deadFilter;
        private EcsWorld world;
        
        public void Init(IEcsSystems systems)
        {
            world = systems.GetWorld();
            deadFilter = world.Filter<UnitData>()
                .Inc<DeadFlag>()
                .End();
            aliveFilter = world.Filter<UnitData>()
                .Exc<DeadFlag>()
                .End();
            unitPool = world.GetPool<UnitData>();
        }

        public void PostRun(IEcsSystems systems)
        {
            int deadCount = deadFilter.GetEntitiesCount();
            foreach (var entity in deadFilter)
            {
                Dispose(entity, unitPool.Get(entity));
            }

            if (deadCount > 0)
            {
                foreach (var entity in aliveFilter)
                {
                    var unit = unitPool.Get(entity);
                    shareData.Simulator.PauseAgent(unit.agentId, false);
                }
            }
        }

        void Dispose(int e, UnitData unit)
        {
            world.DelEntity(e);
            shareData.Simulator.RemoveAgent(unit.agentId);
            Shape.Remove(unit.shapeId);
        }
    }
}