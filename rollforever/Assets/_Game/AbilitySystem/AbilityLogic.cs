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
        private readonly EcsFilter filter;
        private readonly EcsPool<UnitData> unitPool;
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

            filter = world.Filter<UnitData>()
                .Exc<DeadFlag>()
                .End();
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

            bool hit = false;
            
            shapeLogic.PreExecute();
            
            // todo: xử dụng grid để giảm lượng call.
            foreach (var entity in filter)
            {
                ref var unit = ref unitPool.Get(entity);
                ShapeInstance.TryGet(unit.shapeId, out var target);
                float2 targetPos = shareData.Simulator.GetAgentPosition(unit.agentId);
                shapeLogic.Execute(center, target, targetPos, deltaTime, out hit);
                if (hit)
                {
                    deadPool.Add(entity);
                }
            }
            
            shapeLogic.AfterExecute(center);

#if UNITY_EDITOR && DEVELOP_MODE
            Color color = hit ? Color.red : Color.green;
            if (data.shape.type == ShapeType.Circle)
            {
                GeometryGizmos.DrawCircle(
                    new Circle(center, data.shape.radius),
                    color, deltaTime
                );
            }
            else if (data.shape.type == ShapeType.Box)
            {
                GeometryGizmos.DrawAABB(
                    AABB.FromCenter(center, data.shape.size),
                    color, deltaTime
                );
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
}