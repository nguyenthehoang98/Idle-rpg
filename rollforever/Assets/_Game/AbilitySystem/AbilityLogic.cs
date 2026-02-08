using System;
using _Game.Battle;
using _Game.Battle.Data;
using _KIT.Utils;
using Geometry;
using Geometry.Primary;
using Leopotam.EcsLite;
using RVO;
using Unity.Collections;
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
        private readonly ShapeLogic shapeLogic;
        private readonly TrajectoryLogic trajectoryLogic;
        private readonly EcsPool<UnitData> unitPool;
        private readonly int unitId;
        
        private float lifeTime;
        private float elapsed;

        public AbilityLogic(AbilityData data, EcsWorld world,
            BattleStartupShareData shareData, BattleStartupRuntimeData runtimeData,
            int unitId)
        {
            this.world = world;
            this.runtimeData = runtimeData;
            this.shareData = shareData;
            this.unitId = unitId;
            this.data = data;
            shapeLogic = new ShapeLogic(data.shape);
            trajectoryLogic = new TrajectoryLogic(data.trajectory);
            lifeTime = data.arg.lifeTime;

            var shapePool = world.GetPool<ShapeData>();
            var deadPool = world.GetPool<DeadFlag>();
            unitPool = world.GetPool<UnitData>();
            monsterVisitor = new MonsterCellVisitor(
                shareData.Simulator, shapeLogic,
                unitPool, shapePool, deadPool);
        }

        public void Startup(float2 startPos, int target)
        {
            var unit = unitPool.Get(target);
            var targetPos = shareData.Simulator.GetAgentPosition(unit.agentId);
            trajectoryLogic.Startup(startPos, targetPos);
            shapeLogic.Startup(startPos);
        }

        public void Update(float deltaTime)
        {
            elapsed += deltaTime;
            float2 center = trajectoryLogic.Update(deltaTime);

            shapeLogic.PreExecute();
            monsterVisitor.PreVisit(center, deltaTime);
            
            if (data.shape.type == ShapeType.Box)
            {
                shareData.Matrix.ScanArea(center, data.shape.size, monsterVisitor);
            }
            else if (data.shape.type == ShapeType.Circle)
            {
                shareData.Matrix.ScanArea(center, data.shape.radius, monsterVisitor);
            }
            else
            {
                throw new NotImplementedException("Shape chưa được xác định");
            }
            
            shapeLogic.AfterExecute(center);
            
            monsterVisitor.AfterVisit();
            
            
#if UNITY_EDITOR && DEVELOP_MODE
            Color color = monsterVisitor.Hit ? Color.red : Color.green;
         
            Debug.DrawLine((Vector2) shapeLogic.prevPos, (Vector2) center, color, deltaTime);
            if (data.shape.type == ShapeType.Circle)
            {
                GeometryGizmos.DrawCircle(new Circle(center, data.shape.radius), color, deltaTime);
            }
            else if (data.shape.type == ShapeType.Box)
            {
                GeometryGizmos.DrawBox(Box.FromCenter(center, data.shape.size), color, deltaTime);
            }
#endif
        }

        public void Shutdown()
        {
            trajectoryLogic.Shutdown();
            trajectoryLogic.Shutdown();
        }

        public void Dispose()
        {
            shapeLogic.Dispose();
            trajectoryLogic.Dispose();
        }

        public bool IsCompleted => elapsed >= lifeTime;

        public AbilityLogic CreateInstance(int sourceId)
        {
            return new AbilityLogic(data, world, shareData, runtimeData, sourceId);
        }
    }

    class MonsterCellVisitor : ICellVisitor
    {
        private readonly EcsPool<UnitData> unitPool;
        private readonly EcsPool<ShapeData> shapePool;
        private readonly EcsPool<DeadFlag> deadPool;
        private readonly Simulator simulator;
        private readonly ShapeLogic shapeLogic;
        private NativeList<int> entityVisitedStamp;
        private float2 center;
        private float deltaTime;
        
        public int CurrentScanId;
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
        
        public void PreVisit(float2 center, float deltaTime)
        {
            this.center = center;
            this.deltaTime = deltaTime;
            this.CurrentScanId = 0;
            this.Hit = false;
        }
        
        public void Visit(int entity)
        {
            if (!unitPool.Has(entity)) 
                return;

            for (int i = 0; i < CurrentScanId; i++)
            {
                if (entityVisitedStamp[i] == entity)
                    return;
            }

            if(entityVisitedStamp.Length > CurrentScanId)
                entityVisitedStamp[CurrentScanId] = entity;
            else 
                entityVisitedStamp.Add(entity);
            CurrentScanId++;
            
            var unit = unitPool.Get(entity);
            var shape = shapePool.Get(entity);
            float2 agentPos = simulator.GetAgentPosition(unit.agentId);
            
            shapeLogic.Execute(center, shape.Value, agentPos, deltaTime, out Hit);
            if (Hit)
                deadPool.Add(entity);
        }

        public void AfterVisit()
        {
        }
    }
}