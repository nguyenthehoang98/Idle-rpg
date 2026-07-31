using System;
using System.Collections.Generic;
using _GameToolkit.Resource;
using _GameToolkit.SkillSystem.Core;
using _GameToolkit.SkillSystem.Imp;
using _GameToolkit.Updater;
using _TDS.Unit;
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
            GameObject prefab = await AssetManager.GetAssetCached<GameObject>(skillData.prefabName);

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
