using System;
using System.Collections.Generic;
using _TDS.GameplayScene.Unit;
using _TDS.GameplayScene.View;
using _TDS.Utils;
using _Toolkit.Avoidance;
using _Toolkit.Entities;
using _Toolkit.ResourceManagement;
using _Toolkit.Shared;
using _Toolkit.SkillSystem.Core;
using _Toolkit.SkillSystem.Implement;
using UnityEngine;

namespace _TDS.GameplayScene.SkillSystem
{
    public static class SkillFactory
    {
        private static Dictionary<int, CoroutineHandle> PredictedHealthCoroutines = new Dictionary<int, CoroutineHandle>();
        private static HashSet<string> assetPaths = new HashSet<string>();

        private static SkillTickRunner Runner;
        private static int NextActionId = 0;
        
        public static void Initialize(SkillTickRunner runner)
        {
            SkillFactory.Runner = runner;
            SkillFactory.NextActionId = 0;
        }
        
        public static void BuildSkill(SkillContext context)
        {
            if (context.Fire.UseEquipment)
            {
                // Duration tính từ quãng đường / tốc độ (config chỉ set Speed)
                float distance = Vector3.Distance(context.Fire.EquipmentMuzzle, context.Fire.ProjectileDestination);
                context.Projectile.Duration = distance / Mathf.Max(0.01f, context.Projectile.Speed);

                BuildSkill_Private(
                    context, context.Fire.EquipmentMuzzle, context.Fire.ProjectileDestination, 1
                );
                return;
            }

            PredictedTargetDamage(context);

            bool shouldContinue = true;

            Vector3 destination = context.Fire.ProjectileDestination;
            Vector3 position = context.Fire.EquipmentMuzzle;
            Vector3 direction = (destination - position).normalized;
            Vector3 perpendicular = new Vector3(-direction.y, direction.x, 0);

            if (context.SpreadProjectileCount > 0)
            {
                int count = context.SpreadProjectileCount;

                float d = Mathf.CeilToInt(context.SpreadProjectileCount / 2f) * context.ProjectileDistanceStep;

                Vector3 left = position - d * perpendicular;

                int mid = count / 2;

                Vector3 right = position + d * perpendicular;

                float scaleDamage = context.SpreadDamageScale;

                for (int i = 0; i < count; i++)
                {
                    float angle = (i - (count - 1) * 0.5f) * context.ProjectileAngleStep / Mathf.Max(1, count - 1);

                    Vector3 dir = Quaternion.Euler(0, 0, angle) * direction;

                    Vector3 final;

                    if (i >= count / 2) final = right;

                    else final = left;

                    BuildSkill_Private(context, final, final + dir * 100f, mid == i ? 1 : scaleDamage);
                }

                shouldContinue = false;
            }

            if (context.ParallelProjectileCount > 0)
            {
                int count = context.ParallelProjectileCount + 1;

                float scaleDamage = context.ParallelDamageScale;

                float space = context.ProjectileDistanceStep;

                float size = context.Projectile.SizeScale;

                for (int i = 0; i < count; i++)
                {
                    float offset = (i - (count - 1) * 0.5f) * space * size;

                    Vector3 offsetPos = position + perpendicular * offset;

                    Vector3 offsetDest = destination + perpendicular * offset;

                    BuildSkill_Private(context, offsetPos, offsetDest, scaleDamage);
                }

                shouldContinue = false;
            }

            if (!shouldContinue)
            {
                BuildSkill_Private(context, position, destination, 1);
            }
        }

        private static async void BuildSkill_Private(SkillContext context, Vector3 from,
            Vector3 to, float damageScale)
        {
            Projectile projectile;
            
            if (context.Fire.UseEquipment)
            {
                // Equipment.projectile là reference tới prefab asset -> phải instantiate, không dùng trực tiếp
                GameObject equipmentPrefab = context.Fire.Equipment.projectile.gameObject;

                Pool.RegisterPool(equipmentPrefab, true); // idempotent

                projectile = Pool.Instantiate(equipmentPrefab, from, false).GetComponent<Projectile>();
            }
            else
            {
                GameObject go = await AssetLoader.GetAsset<GameObject>(context.AssetName);
                go = Pool.Instantiate(go, from, false);
                projectile = go.GetComponent<Projectile>();
            }
            
            projectile.Initialize(from, to, context.Projectile);
            projectile.EnsureValid(out float lifetime);
            projectile.OnPhaseChanged += context.Fire.Equipment.OnProjectilePhaseChange;

            ProjectileSkillAction skillAction = new ProjectileSkillAction(
                lifetime, projectile.CollisionDetectors(), 
                context.Damage.DamageInterval, context.Damage.HitInterval, context.Damage.HitCount
            );

            skillAction.OnDamaged += info => OnDamagedFunc(context, info, projectile, damageScale);
            skillAction.OnComplete += () =>
            {
                if (context.ResetTiming == AttackResetTiming.OnSkillFinished)
                {
                    context.Fire.Equipment.StopAttack();
                }
            };

            Runner.QueueAddSkillAction(NextActionId++, skillAction);
            
            projectile.Startup();
        }

        private static bool OnDamagedFunc(SkillContext context, HitInfo hitInfo, Projectile projectile, float scaleDamage)
        {
            bool alive = MonsterTickRunner.Instance.TryGet(context.TargetEntity, out MonsterTickRunner.Data data);

            if (!alive) return false;
            
            float explosionRadius = Mathf.Max(0, context.ExplosiveRadius);

            if (explosionRadius == 0)
            {
                Vector3 textDamagePosition = hitInfo.Unique.transform.position;
               
                projectile.EvaluateTextDamagePosition(ref textDamagePosition);

                return ValidateCalculatorDamage(
                    context, projectile, textDamagePosition, context.TargetEntity,
                    hitInfo.IsLastHit, scaleDamage
                );
            }
            else
            {
                MonsterTickRunner.Instance.TryGetAgent(data.agent, out AgentData agentData);

                Vector3 explosionPosition = new Vector3(agentData.position.x, agentData.position.y);
                
                SpawnExplosive(context.ExplosiveAssetName, explosionPosition);
                
                ContactFilter2D filter = new ContactFilter2D();
                filter.useLayerMask = false;

                Collider2D[] results = new Collider2D[20];
                
                int count = Physics2D.OverlapCircle(
                    explosionPosition, explosionRadius, filter, results
                );
                
                float explosiveDamagePercent = context.ExplosiveDamageScale;
                
                for (int i = 0; i < count; i++)
                {
                    Unique unique = results[i].GetComponent<Unique>();

                    if (unique != null)
                    {
                        float explosionDamageScale = (unique == hitInfo.Unique ? 1 : explosiveDamagePercent) * scaleDamage;

                        Vector3 textDamagePosition = unique.transform.position;

                        projectile.EvaluateTextDamagePosition(ref textDamagePosition);
                        
                        return ValidateCalculatorDamage(
                            context, projectile, textDamagePosition, unique.Id(),
                            hitInfo.IsLastHit, explosionDamageScale
                        );
                    }
                }

                return true;
            }
        }

        private static bool ValidateCalculatorDamage(SkillContext context, Projectile projectile,
            Vector3 textDamagePosition, int entity,
            bool lastCollision, float scaleDamage
        )
        {
            bool alive = MonsterTickRunner.Instance.TryGet(entity, out MonsterTickRunner.Data data);

            if (!alive) return false;

            ref HealthComponent health = ref ComponentManager<HealthComponent>.Get(entity);

            if (health.CurrentHealth <= 0) return false;

            int damage = Mathf.CeilToInt(DamageFormula.DamageOutput(context, out bool critical) * scaleDamage);

            health.CurrentHealth -= damage;

            float healthPercent = health.CurrentHealth / (float)health.MaxHealth;
            
            bool killInstant = (healthPercent) <= context.InstantKillTargetBelowHealthPercent;

            if (killInstant) damage += health.CurrentHealth;
            
            data.monster.TakeDamagePost(damage);
            
            SpawnTextDamage(damage, critical, killInstant, textDamagePosition);

            if (killInstant || healthPercent <= 0)
            {
                if (lastCollision)
                {
                    projectile.OnDestroy += () =>
                    {
                        MonsterTickRunner.Instance.Remove(data);
                    };
                }
                else
                {
                    MonsterTickRunner.Instance.Remove(data);
                }
            }

            return true;
        }
        
        private static async void SpawnExplosive(string prefabName, Vector3 position)
        {
            if (string.IsNullOrEmpty(prefabName)) return;

            GameObject go = await AssetLoader.GetAssetCached<GameObject>(prefabName);

            if (assetPaths.Add(prefabName)) Pool.RegisterPool(go, true);

            GameObject o = Pool.Instantiate(go);
            
            o.transform.position = position;
        }
        
        private static async void SpawnTextDamage(int damage, bool critical, bool killInstant, Vector3 position)
        {
            string path = Path.TEXT_DAMAGE_NORMAL;

            if (killInstant)
            {
                if (critical) path = Path.TEXT_DAMAGE_CRITICAL_INSTANT_KILL;
                else path = Path.TEXT_DAMAGE_NORMAL_INSTANT_KILL;
            }
            else
            {
                if (critical) path = Path.TEXT_DAMAGE_NORMAL;
            }

            GameObject go = await AssetLoader.GetAssetCached<GameObject>(path);

            TextDamage td = Pool.Instantiate(go).GetComponent<TextDamage>();
            
            float offsetX = RandomUtils.Range(-0.3f, 0.3f);
            
            td.transform.position = position + new Vector3(offsetX, 0, 0);
            
            td.Execute(damage);
        }

        /*
     * @Damage & Hp calculate
     * Phương thức này mục đích là gán HP entity đang bị ngắm & tính hp ngay khi bắn để tránh các vũ khí đều tranh vào 1 entity
     */
        private static void PredictedTargetDamage(SkillContext context)
        {
            int entity = context.TargetEntity;
            
            if (!MonsterTickRunner.Instance.IsAlive(context.TargetEntity)) return;

            context.CritRate = 0;

            int damage = Mathf.CeilToInt(DamageFormula.DamageOutput(context, out bool critical));

            ref HealthComponent health = ref ComponentManager<HealthComponent>.Get(entity);

            health.PredictedHealth -= damage;

            if (PredictedHealthCoroutines.TryGetValue(entity, out CoroutineHandle coroutine))
            {
                Timing.Instance.KillCoroutinesOnInstance(coroutine);
            }

            PredictedHealthCoroutines[entity] = Timing.Instance.CallDelayedOnInstance(1, () =>
            {
                if (!MonsterTickRunner.Instance.IsAlive(context.TargetEntity)) return;

                ref HealthComponent health = ref ComponentManager<HealthComponent>.Get(entity);

                health.PredictedHealth = health.CurrentHealth;
            });
        }

        public static void Dispose() => SkillFactory.Runner = null;
    }
}







/*
using System;
using System.Collections.Generic;
using _GameToolkit.SkillSystem.Core;
using _GameToolkit.SkillSystem.Imp;
using _GameToolkit.Updater;
using _TDS.Unit;
using _Toolkit.ResourceManagement;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _TDS.Skill
{
    /// <summary>
    /// Orchestrator skill: kế thừa BaseUpdatable (tick loop) + composition Spu (action queue),
    /// cast skill → build trajectory + colliders → CastProjectileAction → damage monster.
    ///
    /// EntityId = agent ID từ RVO. Monster registry qua MonsterMoveUpdater.Instance.
    /// </summary>
    public class SkillManager : BaseUpdatable
    {
        [SerializeField] private Transform projectileParent;
        [SerializeField] private TrajectoryData defaultTrajectory;

        private readonly Spu spu = new Spu();
        private IQuery query;

        private readonly HashSet<string> registeredPools = new HashSet<string>();

        public event Action<HitInfo, int> OnDamageDealt; // (hit, damage)

        private void Awake()
        {
            query = new EntityQuery();
        }

        public override void Tick(float deltaTime)
        {
            spu.Tick(deltaTime);
        }

        public void CastSkill(SkillData skillData, SkillRuntimeData runtimeData)
        {
            Vector3 position = runtimeData.Muzzle;
            Vector3 destination = runtimeData.Destination;

            CastSkill_Private(skillData, runtimeData, position, destination, 1f);
        }

        private async void CastSkill_Private(SkillData skillData, SkillRuntimeData runtimeData,
            Vector3 position, Vector3 destination, float scaleDamage)
        {
            float lifeTime = 0;

            BaseTrajectory trajectory = GetTrajectory(runtimeData, skillData, ref position, ref destination, ref lifeTime);
            if (trajectory == null) return;

            List<BaseCollider> colliders = new List<BaseCollider>();

            // Load projectile prefab
            GameObject prefab = await AssetLoader.GetAssetCached<GameObject>(skillData.prefabName);

            if (prefab == null)
            {
                Debug.LogError($"Stop cast skill because prefab is null, skillId {skillData.skillId}");
                return;
            }

            if (registeredPools.Add(skillData.prefabName))
            {
                Pool.RegisterPool(prefab, true);
            }

            GameObject go = Pool.Instantiate(prefab, position, false);
            if (projectileParent != null) go.transform.SetParent(projectileParent);

            Projectile projectile = go.GetComponent<Projectile>();

            if (projectile == null)
            {
                Debug.LogError($"Stop cast skill because Projectile Component is null, prefab {prefab.name}");
                return;
            }

            projectile.Rotate(destination - position);
            projectile.Initialize(runtimeData);

            float projectileScale = runtimeData.ProjectileScaleBonus + 1;
            projectile.SetSizeScale(projectileScale);

            for (int i = 0; i < projectile.Colliders.Length; i++)
            {
                BaseCollider collider = GetCollider(projectile.Colliders[i], skillData);

                if (collider != null) colliders.Add(collider);
            }

            int maxHitCount = skillData.maxHitCount <= 0 ? int.MaxValue : skillData.maxHitCount;
            maxHitCount += runtimeData.BonusPierceCount;

            CastProjectileAction castProjectileAction = new CastProjectileAction(
                lifeTime, colliders, trajectory,
                info => OnDamageEntityFunction(skillData, runtimeData, info, scaleDamage),
                projectile, skillData.damageTickInterval, maxHitCount, skillData.targetHitCooldown);

            castProjectileAction.OnComplete += () => projectile.Destroy();

            spu.RequestAddAction(1, castProjectileAction);

            projectile.ImmediatelySetPosition(trajectory.EvaluatePosition(0));

            projectile.Active();
        }

        private bool OnDamageEntityFunction(SkillData skillData, SkillRuntimeData runtimeData,
            HitInfo info, float scaleDamage)
        {
            if (MonsterMoveUpdater.Instance == null) return false;

            if (!MonsterMoveUpdater.Instance.TryGetMonster(info.EntityId, out Monster monster)) return false;

            if (!monster.IsAlive) return false;

            float radius = Mathf.Max(0, runtimeData.ExplosiveRadius);

            if (radius > 0)
            {
                Vector2 size = Vector2.one * radius / 2f;

                List<int> entities = query.GetAllEntities(monster.transform.position, size, id => true);

                float sqr = radius * radius;

                foreach (var entity in entities)
                {
                    if (!MonsterMoveUpdater.Instance.TryGetMonster(entity, out Monster m)) continue;

                    float d = Vector3.SqrMagnitude(m.transform.position - monster.transform.position);

                    if (d < sqr)
                    {
                        float dmg = (entity == info.EntityId ? 1 : runtimeData.ExplosiveDamagePercent) * scaleDamage;
                        ApplyDamage(m, Mathf.RoundToInt(runtimeData.Attack * dmg), info);
                    }
                }
            }
            else
            {
                int damage = Mathf.RoundToInt(runtimeData.Attack * scaleDamage);
                ApplyDamage(monster, damage, info);
            }

            return true;
        }

        private void ApplyDamage(Monster monster, int damage, HitInfo info)
        {
            monster.TakeDamage(damage);
            OnDamageDealt?.Invoke(info, damage);
        }

        private BaseCollider GetCollider(ColliderData colliderData, SkillData skillData)
        {
            switch (colliderData.type)
            {
                case ColliderType.Circle:
                    return new CircleCollider(query, colliderData.relativePosition,
                        skillData.collisionStartDelay, skillData.collisionDuration, colliderData.circleRadius);
                case ColliderType.Rectangle:
                    return new RectangleCollider(query, colliderData.relativePosition,
                        skillData.collisionStartDelay, skillData.collisionDuration, colliderData.rectangleSize,
                        colliderData.dependencyRelativeRotation);
                default:
                    Debug.LogError("Unknown Collider type " + colliderData.type);
                    return null;
            }
        }

        private BaseTrajectory GetTrajectory(SkillRuntimeData runtimeData, SkillData skillData,
            ref Vector3 position, ref Vector3 destination, ref float duration)
        {
            TrajectoryData trajectoryData = runtimeData.Trajectory ?? defaultTrajectory;

            switch (skillData.trajectory)
            {
                case TrajectoryType.Projectile:
                    duration = skillData.projectileDuration;

                    float distance = Vector3.Distance(position, destination);
                    float speed = distance / Mathf.Max(0.01f, duration);

                    return new ProjectileTrajectory(trajectoryData.projectileCurve, speed, duration, position, destination);

                case TrajectoryType.Boomerang:
                    duration = skillData.boomerangOutboundDuration + skillData.boomerangHangDuration +
                               skillData.boomerangReturnDuration;

                    return new BoomerangTrajectory(trajectoryData.boomerangInitCurve,
                        trajectoryData.boomerangReturnCurve, skillData.boomerangOutboundDuration,
                        skillData.boomerangHangDuration, skillData.boomerangReturnDuration,
                        position, destination);

                case TrajectoryType.Spline:
                    duration = skillData.splineWindupDuration + skillData.splineExecuteDuration +
                               skillData.splineRecoveryDuration;

                    if (trajectoryData.spline == null || trajectoryData.spline.Spline == null)
                    {
                        Debug.LogError($"Skill {skillData.skillId} cần SplineContainer cho trajectory Spline");
                        return null;
                    }

                    return new SplineTrajectory(trajectoryData.spline.Spline,
                        skillData.splineWindupDuration, skillData.splineExecuteDuration,
                        skillData.splineRecoveryDuration, position, destination);

                case TrajectoryType.Stationary:
                    duration = skillData.stationaryDuration;

                    switch (skillData.stationaryPivot)
                    {
                        case TrajectoryStationaryPivot.Enemy:
                            position = runtimeData.Muzzle;
                            destination = runtimeData.Destination;
                            break;
                        case TrajectoryStationaryPivot.Random:
                            position = runtimeData.Muzzle;
                            destination = position + (Vector3)(Random.insideUnitCircle * skillData.stationaryRandomRadius);
                            break;
                        case TrajectoryStationaryPivot.Weapon:
                            position = runtimeData.Pivot;
                            destination = runtimeData.Muzzle;
                            break;
                    }

                    return new StationaryTrajectory(position, destination);

                default:
                    Debug.LogError("Unknown Trajectory type " + skillData.trajectory);
                    return null;
            }
        }
    }
}
*/
