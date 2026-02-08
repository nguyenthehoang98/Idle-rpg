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
    public sealed class AbilityLogic : IDisposable
    {
        private readonly AbilityData data;
        private readonly EcsWorld world;
        private readonly EcsFilter monsterFilter;
        private readonly EcsPool<UnitData> unitPool;
        private readonly EcsPool<ShapeData> shapePool;
        private readonly EcsPool<DeadFlag> deadPool;
        private readonly BattleStartupRuntimeData runtimeData;
        private readonly BattleStartupShareData shareData;
        private readonly ShapeLogic shapeLogic;
        private readonly TrajectoryLogic trajectoryLogic;
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

            monsterFilter = world.Filter<UnitData>()
                .Inc<ShapeData>()
                .Inc<MonsterFlag>()
                .Exc<DeadFlag>()
                .End();
            shapePool = world.GetPool<ShapeData>();
            deadPool = world.GetPool<DeadFlag>();
            unitPool = world.GetPool<UnitData>();
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

            ExeMonster(center, deltaTime);
            ExePlayer(center, deltaTime);
            
            shapeLogic.AfterExecute(center);
        }

        private void ExeMonster(float2 center, float deltaTime)
        {
            // todo: sử dụng grid để giảm lượng call, ở đấy cần xử lý, Matrix.Cell.agentList
            bool hit = false;
            foreach (var entity in monsterFilter)
            {
                var unit = unitPool.Get(entity);
                var shape = shapePool.Get(entity);
                float2 agentPos = shareData.Simulator.GetAgentPosition(unit.agentId);
                shapeLogic.Execute(center, shape.Value, agentPos, deltaTime, out hit);
                if (hit) deadPool.Add(entity);
            }
            
#if UNITY_EDITOR && DEVELOP_MODE
            Color color = hit ? Color.red : Color.green;
         
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

        private void ExePlayer(float2 center, float deltaTime)
        {
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
}