using _Game.Battle.Data;
using GoodCat.EcsLite.Shared;
using Leopotam.EcsLite;
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
            foreach (var e in ecsFilter)
            {
                var unit = unitPool.Get(e);
                shareData.Simulator.SyncAgentVelocity(unit.agentId);
            }
            
            shareData.Simulator.DoStep();
        }
    }
}