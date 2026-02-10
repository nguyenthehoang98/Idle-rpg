using System;
using _Game.Battle;
using _Game.Battle.Data;
using _KIT.Utils;
using Geometry;
using Geometry.Primary;
using Leopotam.EcsLite;
using Unity.Mathematics;
using UnityEngine;

namespace _Game.AbilitySystem
{
    public sealed class AbilityLogic : IEcsWorldEventListener, IDisposable
    {
        public readonly AbilityData Data;
        // ecs
        private readonly EcsPool<UnitData> unitPool;
        private readonly EcsPool<UnitPosTempData> unitPosTempPool;
        // model
        private readonly BattleStartupRuntimeData runtimeData;
        private readonly BattleStartupShareData shareData;
        private readonly MonsterCellVisitor monsterVisitor;
        // modules
        private readonly ShapeLogic shapeLogic;
        private readonly StateModifierLogic stateModifierLogic;
        private readonly TrajectoryLogic trajectoryLogic;
        private readonly float lifeTime;
        // runtimes
        private int unitId;
        private float elapsed;

        public AbilityLogic(AbilityData data,
            BattleStartupShareData shareData, BattleStartupRuntimeData runtimeData,
            EcsPool<UnitData> unitPool, EcsPool<ShapeData> shapePool,
            EcsPool<DeadFlag> deadPool, EcsPool<HealthData> healthPool, 
            EcsPool<UnitModifierData> modifierPool, EcsPool<UnitPosTempData> unitPosTempPool)
        {
            this.Data = data;
            this.lifeTime = data.core.lifeTime;
            this.runtimeData = runtimeData;
            this.shareData = shareData;

            this.unitPool = unitPool;
            this.unitPosTempPool = unitPosTempPool;
            
            shapeLogic = new ShapeLogic(data.shape);
            stateModifierLogic = new StateModifierLogic(
                data.stateModifier, shareData.Simulator, unitPool, modifierPool, unitPosTempPool
            );
            trajectoryLogic = new TrajectoryLogic(data.trajectory);
            
            monsterVisitor = new MonsterCellVisitor(
                data.core.maxCollision, data.core.shouldResetCollision, data.core.resetCollisionInterval,
                shareData.Simulator, shapeLogic, stateModifierLogic,
                healthPool, unitPool, shapePool, deadPool);
        }

        public void Startup(int source, float2 startPos, int target)
        {
            unitId = source;
            UnitData targetData = unitPool.Get(target);
            float2 targetPos = shareData.Simulator.GetAgentPosition(targetData.agentId);
            trajectoryLogic.Startup(startPos, targetPos);
            stateModifierLogic.Startup(unitId);
            shapeLogic.Startup(startPos);
        }

        public void Update(float deltaTime)
        {
            elapsed += deltaTime;
            float2 center = trajectoryLogic.Update(deltaTime);

            // todo: pre update
            shapeLogic.PreExecute(deltaTime);

            monsterVisitor.PreVisit(center);
            
            // todo: update
            if (shapeLogic.Shape.type == ShapeType.Box)
            {
                shareData.Matrix.ScanArea(center, shapeLogic.Shape.size, monsterVisitor);
            }
            else if (shapeLogic.Shape.type == ShapeType.Circle)
            {
                shareData.Matrix.ScanArea(center, shapeLogic.Shape.radius, monsterVisitor);
            }
            else
            {
#if DEVELOP_MODE
                throw new Exception($"Shape {shapeLogic.Shape.type} chưa được xác định");        
#endif          
            }
            
            stateModifierLogic.Update(deltaTime);
            
            // todo: late update
            
            shapeLogic.AfterExecute(center);
            
            monsterVisitor.AfterVisit(deltaTime);
            
#if UNITY_EDITOR && DEVELOP_MODE
            Color color = monsterVisitor.IsHit ? Color.red : Color.green;
         
            Debug.DrawLine((Vector2) shapeLogic.PrevPos, (Vector2) center, color, deltaTime);
            if (shapeLogic.Shape.type == ShapeType.Circle)
            {
                GeometryGizmos.DrawCircle(new Circle(center, shapeLogic.Shape.radius), color, deltaTime);
            }
            else if (shapeLogic.Shape.type == ShapeType.Box)
            {
                GeometryGizmos.DrawBox(Box.FromCenter(center, shapeLogic.Shape.size), color, deltaTime);
            }
            else
            {
                Debug.LogError($"Shape {shapeLogic.Shape.type} chưa được xác định");
            }
#endif
        }

        public void Shutdown()
        {
            shapeLogic.Shutdown();
            stateModifierLogic.Shutdown();
            trajectoryLogic.Shutdown();
        }

        public void Dispose()
        {
            shapeLogic.Dispose();
            stateModifierLogic.Shutdown();
            trajectoryLogic.Dispose();
        }

        public bool IsCompleted => elapsed >= lifeTime || monsterVisitor.RemainCanCollision <= 0;

        public AbilityLogic CreateInstance(EcsPool<UnitData> unitPool, EcsPool<ShapeData> shapePool,
            EcsPool<DeadFlag> deadPool, EcsPool<HealthData> healthPool, 
            EcsPool<UnitModifierData> modifierPool, EcsPool<UnitPosTempData> unitPosTempPool)
        {
            return new AbilityLogic(Data, shareData, runtimeData,
                unitPool, shapePool, deadPool, healthPool, modifierPool, unitPosTempPool
            );
        }

        public void OnEntityCreated(int entity)
        {
        }

        public void OnEntityChanged(int entity, short poolId, bool added)
        {
        }

        public void OnEntityDestroyed(int entity)
        {
            stateModifierLogic.OnEntityDestroyed(entity);
        }

        public void OnFilterCreated(EcsFilter filter)
        {
        }

        public void OnWorldResized(int newSize)
        {
        }

        public void OnWorldDestroyed(EcsWorld world)
        {
        }
    }
}