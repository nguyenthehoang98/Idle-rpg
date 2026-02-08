using _Game.Battle;
using _Game.Battle.Data;
using Leopotam.EcsLite;
using RVO;
using Unity.Collections;
using Unity.Mathematics;

namespace _Game.AbilitySystem
{
    class MonsterCellVisitor : ICellVisitor
    {
        private readonly EcsPool<UnitData> unitPool;
        private readonly EcsPool<ShapeData> shapePool;
        private readonly EcsPool<DeadFlag> deadPool;
        private readonly Simulator simulator;
        private readonly ShapeLogic shapeLogic;
        private NativeList<int> entityVisitedStamp;
        private float2 center;
        private int currentScanId;
        public bool Hit;

        public MonsterCellVisitor(
            Simulator simulator, ShapeLogic shapeLogic,
            EcsPool<UnitData> unitPool, EcsPool<ShapeData> shapePool, EcsPool<DeadFlag> deadPool)
        {
            this.entityVisitedStamp = new NativeList<int>(10, Allocator.Persistent);
            this.shapeLogic = shapeLogic;
            this.simulator = simulator;
            this.unitPool = unitPool;
            this.shapePool = shapePool;
            this.deadPool = deadPool;
        }
        
        public void PreVisit(float2 center)
        {
            this.center = center;
            this.currentScanId = 0;
            this.Hit = false;
        }
        
        public void Visit(int entity)
        {
            if (!unitPool.Has(entity)) 
                return;

            for (int i = 0; i < currentScanId; i++)
            {
                if (entityVisitedStamp[i] == entity)
                    return;
            }

            if(entityVisitedStamp.Length > currentScanId)
                entityVisitedStamp[currentScanId] = entity;
            else 
                entityVisitedStamp.Add(entity);
            currentScanId++;
            
            var unit = unitPool.Get(entity);
            var shape = shapePool.Get(entity);
            float2 agentPos = simulator.GetAgentPosition(unit.agentId);
            
            shapeLogic.Execute(center, shape.Value, agentPos, out Hit);
            if (Hit)
                deadPool.Add(entity);
        }

        public void AfterVisit()
        {
        }
    }
}