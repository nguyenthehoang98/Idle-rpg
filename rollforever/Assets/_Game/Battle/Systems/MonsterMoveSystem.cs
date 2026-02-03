using _Game.Battle.Data;
using Leopotam.EcsLite;

namespace _Game.Battle.Systems
{
    public class MonsterMoveSystem : IEcsInitSystem, IEcsRunSystem
    {
        private BattleStartupShareData shareData;
        private EcsPool<UnitPos> unitPosPool;
        private EcsFilter ecsFilter;
        
        public void Init(IEcsSystems systems)
        {
            EcsWorld world = systems.GetWorld();
            ecsFilter = world.Filter<UnitPos>()
                .End();
            unitPosPool = world.GetPool<UnitPos>();
            shareData = systems.GetShared<BattleStartupShareData>();
        }

        public void Run(IEcsSystems systems)
        {
            shareData.Simulator.DoStep();
            shareData.Simulator.EnsureCompleted();
        }
    }
}