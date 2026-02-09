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
    public sealed class AbilityLogic : IDisposable
    {
        private readonly AbilityData data;
        private readonly EcsWorld world;
        private readonly BattleStartupRuntimeData runtimeData;
        private readonly BattleStartupShareData shareData;
        private readonly MonsterCellVisitor monsterVisitor;
        private readonly EcsPool<UnitData> unitPool;
        private readonly int unitId;
        // modules
        private readonly ShapeLogic shapeLogic;
        private readonly StateModifierLogic stateModifierLogic;
        private readonly TrajectoryLogic trajectoryLogic;

        private float lifeTime;
        private float elapsed;

        public AbilityLogic(AbilityData data, EcsWorld world,
            BattleStartupShareData shareData, BattleStartupRuntimeData runtimeData,
            int unitId)
        {
            this.unitId = unitId;
            this.data = data;
            this.world = world;
            this.runtimeData = runtimeData;
            this.shareData = shareData;
            lifeTime = data.core.lifeTime;

            var shapePool = world.GetPool<ShapeData>();
            var deadPool = world.GetPool<DeadFlag>();
            var healthPool = world.GetPool<HealthData>();
            unitPool = world.GetPool<UnitData>();
            
            shapeLogic = new ShapeLogic(data.shape);
            stateModifierLogic = new StateModifierLogic(data.stateModifier, shareData.Simulator, unitPool);
            trajectoryLogic = new TrajectoryLogic(data.trajectory);
            
            monsterVisitor = new MonsterCellVisitor(
                data.core.maxCollision, data.core.shouldResetCollision, data.core.resetCollisionInterval,
                shareData.Simulator, shapeLogic, stateModifierLogic,
                healthPool, unitPool, shapePool, deadPool);
        }

        public void Startup(float2 startPos, int target)
        {
            var unit = unitPool.Get(target);
            var targetPos = shareData.Simulator.GetAgentPosition(unit.agentId);
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

        public AbilityLogic CreateInstance(int sourceId)
        {
            return new AbilityLogic(data, world, shareData, runtimeData, sourceId);
        }
    }
}