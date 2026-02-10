using System;
using _Game.Battle;
using _Game.Battle.Data;
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
        private readonly BattleStartupRuntimeData runtimeData;
        private readonly BattleStartupShareData shareData;
        private readonly EcsPool<UnitData> unitPool;
        private readonly EcsPool<ShapeData> shapePool;
        private readonly EcsPool<DeadFlag> deadPool;
        private readonly EcsPool<HealthData> healthPool;
        private readonly EcsPool<UnitModifierData> modifierPool;
        private readonly EcsPool<UnitPosTempData> unitPosTempPool;
        private readonly EcsFilter playerFilter; 
        // model
        private readonly MonsterVisitor monsterVisitor;
        private readonly IVisitor playerVisitor;
        // modules
        private readonly ShapeLogic shapeLogic;
        private readonly StateModifierLogic stateModifierLogic;
        private readonly TrajectoryLogic trajectoryLogic;
        private readonly float lifeTime;
        // runtimes
        private Team sourceTeam;
        private int unitId;
        private float elapsed;

        public AbilityLogic(AbilityData data, Team sourceTeam,
            BattleStartupShareData shareData, BattleStartupRuntimeData runtimeData,
            EcsPool<UnitData> unitPool, EcsPool<ShapeData> shapePool,
            EcsPool<DeadFlag> deadPool, EcsPool<HealthData> healthPool, 
            EcsPool<UnitModifierData> modifierPool, EcsPool<UnitPosTempData> unitPosTempPool,
            EcsFilter playerFilter)
        {
            this.Data = data;
            this.lifeTime = data.core.lifeTime;
            
            this.shareData = shareData;
            this.unitPool = unitPool;
            this.shapePool = shapePool;
            this.deadPool = deadPool;
            this.healthPool = healthPool;
            this.modifierPool = modifierPool;
            this.unitPosTempPool = unitPosTempPool;
            this.playerFilter = playerFilter;
            
            this.sourceTeam = sourceTeam;
            this.runtimeData = runtimeData;
            
            shapeLogic = new ShapeLogic(data.shape);
            stateModifierLogic = new StateModifierLogic(
                data.stateModifier, shareData.Simulator, unitPool, modifierPool, unitPosTempPool
            );
            trajectoryLogic = new TrajectoryLogic(data.trajectory);
            
            monsterVisitor = new MonsterVisitor(
                data.core.maxCollision, data.core.shouldResetCollision, data.core.resetCollisionInterval,
                shareData.Simulator, shapeLogic, stateModifierLogic,
                healthPool, unitPool, shapePool, deadPool);
            playerVisitor = new PlayerVisitor();
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

            if(sourceTeam == Team.Player)
                PreHandleMonsters(center);
            else if (sourceTeam == Team.Monster)
            {
            }

            if (sourceTeam == Team.Player)
                HandleMonsters(center);
            else
                HandlePlayers(center);
            
            stateModifierLogic.Update(deltaTime);
            
            // todo: late update
            
            shapeLogic.AfterExecute(center);
            
            if (sourceTeam == Team.Player)
                AfterHandleMonsters(deltaTime);
            else if (sourceTeam == Team.Monster)
            {
            }

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

        private void HandlePlayers(float2 center)
        {
            foreach (var e in playerFilter)
            {
                var unit = unitPool.Get(e);
                var shape = shapePool.Get(e);
                float2 agentPos = shareData.Simulator.GetAgentPosition(unit.agentId);
                shapeLogic.Execute(center, shape.Value, agentPos, out bool hit);
                if (hit)
                {
                    ref var health = ref healthPool.Get(e);
                    health.health -= 50;
                }
            }
        }
        
        private void PreHandleMonsters(float2 center)
        {
            monsterVisitor.PreVisit(center);
        }

        private void HandleMonsters(float2 center)
        {
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
        }
        private void AfterHandleMonsters(float deltaTime)
        {
            monsterVisitor.AfterVisit(deltaTime);
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

        public AbilityLogic CreateInstance(Team team)
        {
            return new AbilityLogic(Data, team, shareData, runtimeData,
                unitPool, shapePool, deadPool, healthPool, modifierPool, unitPosTempPool,
                playerFilter);
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