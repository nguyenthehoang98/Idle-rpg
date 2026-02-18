using _Game.Battle;
using _Game.Battle.Data;
using _Game.Configs;
using Leopotam.EcsLite;
using RVO;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace _Game.AbilitySystem
{
    class MonsterVisitor : IVisitor
    {
        private readonly int sourceEntity;
        private readonly SkillConfig.SkillData skillData;
        private readonly EcsPool<UnitData> unitPool;
        private readonly EcsPool<ShapeData> shapePool;
        private readonly EcsPool<DeadFlag> deadPool;
        private readonly EcsPool<HealthData> healthPool;
        private readonly EcsPool<StatData> statPool;
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
            int entity, SkillConfig.SkillData skillData,
            int maxCollision, bool shouldResetCollision, float resetCollisionInterval,
            Simulator simulator, ShapeLogic shapeLogic, StateModifierLogic stateModifierLogic,
            EcsPool<HealthData> healthPool, EcsPool<StatData> statPool,
            EcsPool<UnitData> unitPool, EcsPool<ShapeData> shapePool, EcsPool<DeadFlag> deadPool)
        {
            this.sourceEntity = entity;
            this.skillData = skillData;
            this.shouldResetCollision = shouldResetCollision;
            this.resetCollisionInterval = resetCollisionInterval;
            this.stateModifierLogic = stateModifierLogic;
            this.entitiesCollision = new NativeList<int>(10, Allocator.Persistent);
            this.entityVisitedStamp = new NativeList<int>(10, Allocator.Persistent);
            this.healthPool = healthPool;
            this.statPool = statPool;
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
            {
#if COMBAT_FULL_LOG
                Debug.LogWarning($"MonsterVisitor: {sourceEntity}->{entity}: remain <= 0");
#endif
                return;
            }

            int length = entitiesCollision.Length;
            for (int i = 0; i < length; i++)
            {
                if (entitiesCollision[i] == entity)
                {
#if COMBAT_FULL_LOG
                    Debug.LogWarning($"MonsterVisitor: {sourceEntity}->{entity}: Đã từng va chạm");
#endif
                    return;
                }
            }

            for (int i = 0; i < currentScanId; i++)
            {
                if (entityVisitedStamp[i] == entity)
                {
#if COMBAT_FULL_LOG
                    Debug.LogWarning($"MonsterVisitor: {sourceEntity}->{entity}: Đã từng quét");
#endif
                    return;
                }
            }

            if (entityVisitedStamp.Length > currentScanId)
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
                var targetStat = statPool.Get(entity);
                var sourceStat = statPool.Get(sourceEntity);

                bool findAttack = sourceStat.TryGetValue(StatType.Attack, out Stat attackStat);
                if (!findAttack)
                {
#if DEVELOP_MODE || COMBAT_FULL_LOG
                    Debug.LogError("Không tìm thấy Attack: " + sourceEntity);
#endif
                    return;
                }

                bool findCriticalRate = sourceStat.TryGetValue(StatType.CriticalRate, out Stat criticalRateStat);
                if (!findCriticalRate)
                {
#if DEVELOP_MODE || COMBAT_FULL_LOG
                    Debug.LogError("Không tìm thấy CriticalRate: " + sourceEntity);
#endif
                    return;
                }

                bool findCriticalDamage = sourceStat.TryGetValue(StatType.CriticalDamage, out Stat criticalDamageStat);
                if (!findCriticalDamage)
                {
#if DEVELOP_MODE || COMBAT_FULL_LOG
                    Debug.LogError("Không tìm thấy CriticalDamage: " + sourceEntity);
#endif
                    return;
                }

                bool foundDefense = targetStat.TryGetValue(StatType.Defense, out Stat defenseStat);
                if (!foundDefense)
                {
#if DEVELOP_MODE || COMBAT_FULL_LOG
                    Debug.LogError("Không tìm thấy Defense: " + entity);
#endif
                    return;
                }

                int output = FormulaUtils.Output(
                    attackStat.Value, skillData, criticalRateStat.Value, criticalDamageStat.Value,
                    defenseStat.Value
                );

                ref var health = ref healthPool.Get(entity);
                health.health -= output;
                if (health.health <= 0)
                {
                    if (!deadPool.Has(entity)) deadPool.Add(entity);
                }
                else
                {
                    stateModifierLogic.TriggerTarget(entity);
                }

                RemainCanCollision--;
                entitiesCollision.Add(entity);
            }
            else
            {
#if COMBAT_FULL_LOG
                Debug.LogWarning($"MonsterVisitor: {sourceEntity}->{entity}: Không va chạm");
#endif
            }
        }

        public void VisitCell(int x, int y)
        {
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