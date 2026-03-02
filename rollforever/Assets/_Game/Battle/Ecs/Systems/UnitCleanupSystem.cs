using _Game.Battle.Ecs.Data;
using _Game.Battle.Ecs.Model;
using _Game.Battle.Ecs.View;
using _KIT.Pool;
using Geometry.Primary;
using GoodCat.EcsLite.Shared;
using Leopotam.EcsLite;

namespace _Game.Battle.Ecs.Systems
{
    public class UnitCleanupSystem : IEcsInitSystem, IEcsPostRunSystem
    {
        [EcsInject] private readonly BattleStartupShareData shareData;

        private EcsPool<MonsterAgentData> unitPool;
        private EcsPool<MonsterTempData> unitPosTempPool;
        private EcsPool<ShapeData> shapePool;
        private EcsFilter aliveFilter;
        private EcsFilter deadFilter;
        private EcsWorld world;
        
        public void Init(IEcsSystems systems)
        {
            world = systems.GetWorld();
            deadFilter = world.Filter<MonsterAgentData>()
                .Inc<MonsterTempData>()
                .Inc<MonsterFlag>()
                .Inc<DeadFlag>()
                .End();
            aliveFilter = world.Filter<MonsterAgentData>()
                .Inc<MonsterTempData>()
                .Inc<MonsterFlag>()
                .Exc<DeadFlag>()
                .End();
            unitPool = world.GetPool<MonsterAgentData>();
            shapePool = world.GetPool<ShapeData>();
            unitPosTempPool = world.GetPool<MonsterTempData>();
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

        void Dispose(int e, MonsterAgentData monsterAgent)
        {
            var shape = shapePool.Get(e);

            world.DelEntity(e);

            var position = shareData.Simulator.GetAgentPosition(monsterAgent.agentId);
            shareData.Matrix.TriggerPoint(e, position);

            if (shape.Value.type == ShapeType.Circle)
            {
                shareData.Matrix.RemoveUnit(e, position, shape.Value.radius);   
            }
            else if (shape.Value.type == ShapeType.Box)
            {
                shareData.Matrix.RemoveUnit(e, position, shape.Value.size);   
            }

            shareData.Simulator.RemoveAgent(monsterAgent.agentId);
            
            if(UnitView.TryRelease(e, out UnitView unitView))
            {
                KitPool.Destroy(unitView.gameObject);
            }
        }
    }
}