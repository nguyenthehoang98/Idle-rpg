using _Game.Battle.Data;
using Geometry;
using Leopotam.EcsLite;
using Unity.Mathematics;

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
            foreach (var e in ecsFilter)
            {
                ref var unit = ref unitPosPool.Get(e);
                var goal = unit.goal;
                var pos = shareData.Simulator.GetAgentPosition(unit.agentId);
                var velocity = math.normalize(goal - pos);
                
                shareData.Simulator.SetAgentPrefVelocity(unit.agentId, velocity);
                shareData.Grid.InsertOrUpdate(new BoxData2D(unit.agentId, pos, new float2(0.5f, 0.5f)));
            }
            
            shareData.Simulator.DoStep();
        }
    }
}