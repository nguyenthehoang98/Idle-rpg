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
        private EcsPool<Unit> unitPool;
        private EcsFilter ecsFilter;
        
        public void Init(IEcsSystems systems)
        {
            EcsWorld world = systems.GetWorld();
            ecsFilter = world.Filter<UnitPos>()
                .Inc<Unit>()
                .End();
            unitPool = world.GetPool<Unit>();
            unitPosPool = world.GetPool<UnitPos>();
            shareData = systems.GetShared<BattleStartupShareData>();
        }

        public void Run(IEcsSystems systems)
        {
            foreach (var e in ecsFilter)
            {
                var unit = unitPool.Get(e);
                ref var unitPos = ref unitPosPool.Get(e);
                var goal = unitPos.goal;
                var pos = shareData.Simulator.GetAgentPosition(unit.agentId);
                unitPos.prevPos = unitPos.curPos;
                unitPos.curPos = pos;

                if (ShapeInstance.TryGet(unit.shapeId, out var shapeInstance))
                {
                    shapeInstance.PrevPosition = unitPos.prevPos;
                    shapeInstance.CurrPosition = unitPos.curPos;
                }

                shareData.Simulator.SetAgentPrefVelocity(unit.agentId, math.normalize(goal - pos));
                shareData.Grid.InsertOrUpdate(
                    new GridObject(unit.cellId, pos, new float2(1, 1) * shapeInstance.Radius * 0.5f)
                );
            }
            
            shareData.Simulator.DoStep();
        }
    }
}