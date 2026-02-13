using _Game.Battle;
using _Game.Battle.Data;
using Leopotam.EcsLite;
using RVO;
using Unity.Collections;
using Unity.Mathematics;

namespace _Game.AbilitySystem
{
    class MonsterVisitor : IVisitor
    {
        private readonly EcsPool<UnitData> unitPool;
        private readonly EcsPool<ShapeData> shapePool;
        private readonly EcsPool<DeadFlag> deadPool;
        private readonly EcsPool<HealthData> healthPool;
        private readonly Simulator simulator;
        private readonly ShapeLogic shapeLogic;
        private readonly StateModifierLogic stateModifierLogic;
        private readonly bool shouldResetCollision;
        private readonly float resetCollisionInterval;
        private NativeList<int> entityVisitedStamp;
        private NativeList<int> entitiesCollision;
        private float2 center;
        private int currentScanId;
        private float elapsed;
        
        public bool IsHit;
        public int RemainCanCollision { get; private set; }

        public MonsterVisitor(
            int maxCollision, bool shouldResetCollision, float resetCollisionInterval,
            Simulator simulator, ShapeLogic shapeLogic, StateModifierLogic stateModifierLogic,
            EcsPool<HealthData> healthPool,
            EcsPool<UnitData> unitPool, EcsPool<ShapeData> shapePool, EcsPool<DeadFlag> deadPool)
        {
            this.shouldResetCollision = shouldResetCollision;
            this.resetCollisionInterval = resetCollisionInterval;
            this.stateModifierLogic = stateModifierLogic;
            this.entitiesCollision = new NativeList<int>(10, Allocator.Persistent);
            this.entityVisitedStamp = new NativeList<int>(10, Allocator.Persistent);
            this.healthPool = healthPool;
            this.shapeLogic = shapeLogic;
            this.simulator = simulator;
            this.unitPool = unitPool;
            this.shapePool = shapePool;
            this.deadPool = deadPool;
            this.RemainCanCollision = maxCollision;
        }
        
        public void PreVisit(float2 center)
        {
            this.center = center;
            this.currentScanId = 0;
            this.IsHit = false;
        }
        
        public void Visit(int entity)
        {
            if (RemainCanCollision <= 0)
                return;
            
            if (!unitPool.Has(entity)) 
                return;

            int length = entitiesCollision.Length;
            for (int i = 0; i < length; i++)
            {
                if (entitiesCollision[i] == entity) return;
            }

            for (int i = 0; i < currentScanId; i++)
            {
                if (entityVisitedStamp[i] == entity) return;
            }

            if(entityVisitedStamp.Length > currentScanId)
                entityVisitedStamp[currentScanId] = entity;
            else 
                entityVisitedStamp.Add(entity);
            currentScanId++;
            
            var unit = unitPool.Get(entity);
            var shape = shapePool.Get(entity);
            float2 agentPos = simulator.GetAgentPosition(unit.agentId);
            
            shapeLogic.Execute(center, shape.Value, agentPos, out IsHit);
            if (IsHit)
            {
                ref var health = ref healthPool.Get(entity);
                health.health -= 100;
                if (health.health <= 0)
                {
                    if(!deadPool.Has(entity)) deadPool.Add(entity);
                }
                else
                {
                    stateModifierLogic.TriggerTarget(entity);
                }

                RemainCanCollision--;
                entitiesCollision.Add(entity);
            }
        }

        public void AfterVisit(float deltaTime)
        {
            elapsed += deltaTime;
            if (shouldResetCollision && elapsed >= resetCollisionInterval)
            {
                elapsed = 0;
                entitiesCollision.Clear();
            }
        }
    }
}