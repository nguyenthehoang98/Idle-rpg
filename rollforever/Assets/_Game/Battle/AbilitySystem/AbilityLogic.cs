using System;
using _Game.Battle.Ecs.Data;
using _Game.Battle.Ecs.Events;
using _Game.Battle.Ecs.Model;
using _Game.Battle.Ecs.View;
using _Game.Scripts.Configs;
using _Game.Scripts.Model;
using _KIT.Event;
using _KIT.Pool;
using _KIT.Utils;
using Geometry;
using Geometry.Primary;
using Leopotam.EcsLite;
using Unity.Mathematics;
using UnityEngine;

namespace _Game.Battle.AbilitySystem
{
    public sealed class AbilityLogic : IEcsWorldEventListener, IDisposable
    {
        public readonly AbilitySO AbilitySo;
        private readonly SkillConfig.SkillData SkillData;
        // ecs
        private readonly BattleStartupShareData shareData;
        private readonly EcsPool<UnitData> unitPool;
        private readonly EcsPool<ShapeData> shapePool;
        private readonly EcsPool<DeadFlag> deadPool;
        private readonly EcsPool<HealthData> healthPool;
        private readonly EcsPool<UnitModifierData> modifierPool;
        private readonly EcsPool<UnitPosTempData> unitPosTempPool;
        private readonly EcsPool<StatData> statPool;
        private readonly EcsFilter playerFilter; 
        // model
        private readonly MonsterVisitor monsterVisitor;
        // modules
        private readonly ShapeLogic shapeLogic;
        private readonly StateModifierLogic stateModifierLogic;
        private readonly TrajectoryLogic trajectoryLogic;
        private readonly float lifeTime;
        private readonly BulletView bulletPrefab;
        // runtimes
        private BulletView bulletInstance;
        private float2 prevPosition;
        private Team sourceTeam;
        private int unitId;
        private float elapsed;
        private bool isHitPlayer;

        public AbilityLogic(AbilitySO abilitySo, SkillConfig.SkillData skillData,
            int entity, Team sourceTeam, BattleStartupShareData shareData, 
            EcsPool<UnitData> unitPool, EcsPool<ShapeData> shapePool,
            EcsPool<DeadFlag> deadPool, EcsPool<HealthData> healthPool, 
            EcsPool<StatData> statPool,
            EcsPool<UnitModifierData> modifierPool, EcsPool<UnitPosTempData> unitPosTempPool,
            EcsFilter playerFilter)
        {
            this.SkillData = skillData;
            this.AbilitySo = abilitySo;
            this.lifeTime = abilitySo.core.lifeTime;
            this.bulletPrefab = abilitySo.core.bulletPrefab;
            
            this.shareData = shareData;
            this.statPool = statPool;
            this.unitPool = unitPool;
            this.shapePool = shapePool;
            this.deadPool = deadPool;
            this.healthPool = healthPool;
            this.modifierPool = modifierPool;
            this.unitPosTempPool = unitPosTempPool;
            this.playerFilter = playerFilter;
            
            this.sourceTeam = sourceTeam;
            
            shapeLogic = new ShapeLogic(abilitySo.shape);
            stateModifierLogic = new StateModifierLogic(
                abilitySo.stateModifier, shareData.Simulator, unitPool, modifierPool, unitPosTempPool
            );
            trajectoryLogic = new TrajectoryLogic(abilitySo.trajectory);
            
            monsterVisitor = new MonsterVisitor(
                entity, skillData,
                abilitySo.core.maxCollision, abilitySo.core.shouldResetCollision, abilitySo.core.resetCollisionInterval,
                shareData.Simulator, shapeLogic, stateModifierLogic,
                healthPool, statPool, unitPool, shapePool, deadPool);
        }

        public void Startup(int source, float2 startPos, bool isTargetValid, int target)
        {
            unitId = source;
            float2 targetPos;
            if (isTargetValid)
            {
                UnitData targetData = unitPool.Get(target);
                targetPos = shareData.Simulator.GetAgentPosition(targetData.agentId);
            }
            else
            {
                targetPos = new float2(RandomUtils.Value, RandomUtils.Value);
                targetPos = math.normalize(targetPos) * 50;
            }

            trajectoryLogic.Startup(startPos, targetPos);
            stateModifierLogic.Startup(unitId);
            shapeLogic.Startup(startPos);

            if (bulletPrefab != null)
            {
                bulletInstance = KitPool.Instantiate(bulletPrefab);
                bulletInstance.Init(startPos, shareData);
            }
        }

        public void Update(float deltaTime)
        {
            elapsed += deltaTime;
            float2 center = trajectoryLogic.Update(deltaTime);

            if (bulletInstance != null)
            {
                bulletInstance.UpdatePosition(center);
            }

            // todo: pre update
            shapeLogic.PreExecute(deltaTime);

            if (sourceTeam == Team.Player)
                PreHandleMonsters(center);

            if (sourceTeam == Team.Player)
                HandleMonsters(prevPosition, center);
            else if (sourceTeam == Team.Monster)
                HandlePlayer(center);

            stateModifierLogic.Update(deltaTime);

            // todo: late update

            shapeLogic.AfterExecute(center);

            if (sourceTeam == Team.Player)
                AfterHandleMonsters(deltaTime);

            prevPosition = center;

#if UNITY_EDITOR && DEVELOP_MODE
            Color color = monsterVisitor.IsHit ? Color.red : Color.green;

            Debug.DrawLine((Vector2)shapeLogic.PrevPos, (Vector2)center, color, deltaTime);
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

        private void HandlePlayer(float2 center)
        {
            foreach (var e in playerFilter)
            {
                var shape = shapePool.Get(e);
                shapeLogic.Execute(center, shape.Value, shareData.PlayerPosition, out bool hit);
                if (hit)
                {
                    int dmg = 10;
                    ref var health = ref healthPool.Get(e);
                    health.health = math.max(health.health - dmg, 0);

                    var stat = statPool.Get(e);
                    stat.TryGetValue(StatType.MaxHealth, out var healthStat);
                    
                    EventBus.Instance.Publish(new DamagePlayerEvent(dmg, health.health, (int)healthStat.Value));

                    isHitPlayer = true;
                }
            }
        }
        
        private void PreHandleMonsters(float2 center)
        {
            monsterVisitor.PreVisit(center);
        }

        private void HandleMonsters(float2 prev, float2 center)
        {
            // todo: update
            if (shapeLogic.Shape.type == ShapeType.Box)
            {
                shareData.Matrix.ScanArea(prev, center, shapeLogic.Shape.size, monsterVisitor);
            }
            else if (shapeLogic.Shape.type == ShapeType.Circle)
            {
                shareData.Matrix.ScanArea(prev, center, shapeLogic.Shape.radius, monsterVisitor);
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
            if (bulletInstance != null)
            {
                KitPool.Destroy(bulletInstance.gameObject);
                bulletInstance = null;
            }
            
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

        public bool IsCompleted => elapsed >= lifeTime || isHitPlayer || monsterVisitor.RemainCanCollision <= 0;

        public AbilityLogic CreateInstance(int sourceEntity, Team team)
        {
            return new AbilityLogic(AbilitySo, SkillData, sourceEntity, team, shareData, 
                unitPool, shapePool, deadPool, healthPool, statPool, modifierPool,
                unitPosTempPool, playerFilter);
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