using _Game.Battle.Data;
using Geometry;
using GoodCat.EcsLite.Shared;
using Leopotam.EcsLite;
using Unity.Mathematics;
using UnityEngine;

namespace _Game.Battle.Systems
{
    public class MonsterMoveSystem : IEcsInitSystem, IEcsRunSystem
    {
        [EcsInject] private readonly BattleStartupShareData shareData;
        [EcsInject] private readonly BattleStartupRuntimeData runtimeData;
        
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
        }

        public void Run(IEcsSystems systems)
        {
            foreach (var e in ecsFilter)
            {
                var unit = unitPool.Get(e);
                var pos = shareData.Simulator.GetAgentPosition(unit.agentId);
                ref var unitPos = ref unitPosPool.Get(e);
                unitPos.prevPos = unitPos.curPos;
                unitPos.curPos = pos;

                if (ShapeInstance.TryGet(unit.shapeId, out var shapeInstance))
                {
                    shapeInstance.PrevPosition = unitPos.prevPos;
                    shapeInstance.CurrPosition = unitPos.curPos;
                }

                shareData.Grid.InsertOrUpdate(
                    new GridObject(unit.cellId, pos, new float2(1, 1) * shapeInstance.Radius * 0.5f)
                );

                if (runtimeData.TryGet(e, out var unitView))
                {
                    unitView.UpdatePosition(pos);
                }
                
                shareData.Simulator.SyncAgentVelocity(unit.agentId);
            }
            
            shareData.Simulator.DoStep();
        }
    }
}