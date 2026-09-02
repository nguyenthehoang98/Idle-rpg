using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using _Game.Configs;
using _Game.GamePlay.Data;
using _Game.GamePlay.Entity;
using _Game.GamePlay.Model;
using _Game.GamePlay.Utils;
using _KITSystem.Entity;
using _KITSystem.Resource;
using _KITSystem.Schedule;
using _KITSystem.SkillSystem.Core;
using _KITSystem.SkillSystem.Imp;
using _KITSystem.Utils;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Game.GamePlay.Manager
{
    [Serializable]
    public sealed class SkillManager : Spu, ITickable
    {
        static SkillManager instance;

        public event Action<PostDamageParams> OnPostDamage;
        public event Action<PostEarnExpParams> OnPostEarnExp;

        private Dictionary<int, Coroutine> coroutinesResetFutureHealth = new Dictionary<int, Coroutine>();
        private IQuery query;
        private HashSet<string> names;

        public Task Initialize()
        {
            query = new EntityQuery();
            names = new HashSet<string>();

            instance = this;
            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            base.Dispose();

            foreach (var name in names)
            {
                AssetBundleManager.UnCache(name);
            }

            names = null;

            instance = null;
        }

        public static void CastSkill(SkillData skillData, SkillRuntimeData runtimeData)
        {
            Vector3 position = runtimeData.Muzzle;
            Vector3 destination = runtimeData.Destination;
            int entityTarget = runtimeData.Entity;
            
            if (instance == null)
            {
                Debug.LogError("Instance SkillManager is null");
                return;
            }

            // ~todo: force cast skill if weapon can fly
            if (runtimeData.UseWeapon)
            {
                instance.CastSkill_Private(skillData, runtimeData, position, destination, 1);
                
                return;
            }

            float projectileScale = runtimeData.ProjectileScaleBonus + 1;

            instance.PredictedTargetDamage(runtimeData, entityTarget);
            
            Vector3 direction = (destination - position).normalized;
            
            bool extra = false;
            
            Vector3 perpendicular = new Vector3(-direction.y, direction.x, 0);
            
            float parallelSpacing = skillData.parallelDistanceStep;

            if (runtimeData.ProjectilesPerShot > 0)
            {
                int count = runtimeData.ProjectilesPerShot + 1;

                float scaleDamage = 1f;//runtimeData.ParallelDamagePercent;
             
                for (int i = 0; i < count; i++)
                {
                    float offset = (i - (count - 1) * 0.5f) * parallelSpacing * projectileScale;

                    Vector3 offsetPos = position + perpendicular * offset;
                    
                    Vector3 offsetDest = destination + perpendicular * offset;

                    instance.CastSkill_Private(skillData, runtimeData, offsetPos, offsetDest, scaleDamage);
                }
                    
                extra = true;
            }

            if (runtimeData.SpreadProjectileCount > 0)
            {
                int count = runtimeData.SpreadProjectileCount + 1;
                
                float angleStep = skillData.spreadAngleStep;

                float scaleDamage = 1f;//runtimeData.SpreadDamagePercent;

                float d = Mathf.CeilToInt(runtimeData.ProjectilesPerShot / 2f) * parallelSpacing;
                
                Vector3 left = position - d * perpendicular;
                
                int mid = count / 2;
                
                Vector3 right = position + d * perpendicular;
                
                for (int i = 0; i < count; i++)
                {
                    float angle = (i - (count - 1) * 0.5f) * angleStep / Mathf.Max(1, count - 1);
                    
                    Vector3 dir = Quaternion.Euler(0, 0, angle) * direction;
                    
                    Vector3 final;
                    
                    if (i >= count / 2) final = right;
                    
                    else final = left;
                    
                    instance.CastSkill_Private(skillData, runtimeData, final, final + dir * 100f,  mid == i ? 1 : scaleDamage);
                }
                    
                extra = true;
            }
                
            if(!extra) instance.CastSkill_Private(skillData, runtimeData, position, destination, 1);
        }
        
        private async void CastSkill_Private(SkillData skillData, SkillRuntimeData runtimeData, Vector3 position, Vector3 destination, float scaleDamage)
        {
            float lifeTime = 0;
            
            BaseTrajectory trajectory = GetTrajectory(runtimeData, skillData, ref position, ref destination, ref lifeTime);
            if (trajectory == null)
            {
#if UNITY_EDITOR
                Debug.LogError("Stop cast skill because Trajectory is null, skillId " + skillData.skillId);
#endif
                return;
            }
            
            Projectile projectile = null;
           
            List<BaseCollider> colliders = new List<BaseCollider>();
            
            bool useWeapon = runtimeData.UseWeapon;
            
            float projectileScale = runtimeData.ProjectileScaleBonus + 1;
          
            if (useWeapon)
            {
                projectile = runtimeData.Weapon.GetComponent<Projectile>();
          
                if (projectile == null)
                {
#if UNITY_EDITOR
                    Debug.LogError("Stop cast skill because Projectile Component is null, prefab " + runtimeData.Weapon.name);
#endif
                    return;
                }
                
                projectile.SetSizeScale(projectileScale);

                for (int i = 0; i < projectile.Colliders.Length; i++)
                {
                    BaseCollider collider = GetCollider(projectile.Colliders[i], skillData);
                    
                    if (collider == null)
                    {
#if UNITY_EDITOR
                        Debug.LogError("Stop cast skill because Collider is null, skillId " + skillData.skillId);
#endif
                        continue;
                    }
                    
                    colliders.Add(collider);
                }
            }
            else
            {
                GameObject prefab = await AssetBundleManager.GetAssetCached<GameObject>(skillData.prefabName);
                
                if (prefab == null)
                {
#if UNITY_EDITOR
                    Debug.LogError("Stop cast skill because prefab is null, skillId " + skillData.skillId);
#endif
                    return;
                }
                
                // ~ get component in prefab
                projectile = prefab.GetComponent<Projectile>();
                
                if (projectile == null)
                {
#if UNITY_EDITOR
                    Debug.LogError("Stop cast skill because Projectile Component is null, prefab " + prefab.name);
#endif
                    return;
                }
                
                projectile.SetSizeScale(projectileScale);

                for (int i = 0; i < projectile.Colliders.Length; i++)
                {
                    BaseCollider collider = GetCollider(projectile.Colliders[i], skillData);

                    if (collider == null)
                    {
#if UNITY_EDITOR
                        Debug.LogError("Stop cast skill because Collider is null, skillId " + skillData.skillId);
#endif
                        continue;
                    }
                    
                    colliders.Add(collider);
                }
                
                GameObject go = Pool.Instantiate(prefab, position, false);

                projectile = go.GetComponent<Projectile>();
                
                projectile.Rotate(destination - position);
            }
            
            Action onProjectileDestroyed = () => { };
            
            int maxHitCount = skillData.maxHitCount <= 0 ? int.MaxValue : skillData.maxHitCount;
            maxHitCount += runtimeData.BonusPierceCount;

            CastProjectileAction castProjectileAction = new CastProjectileAction(lifeTime, colliders, trajectory,
                info => OnDamageEntityFunction(skillData, runtimeData, info, scaleDamage, ref onProjectileDestroyed),
                projectile, skillData.damageTickInterval, maxHitCount,
                skillData.targetHitCooldown
            );
            
            castProjectileAction.OnComplete += () =>
            {
                projectile.Destroy(onProjectileDestroyed);

                if (runtimeData.DependencyReset == DependencyResetAttack.Skill)
                {
                    runtimeData.Weapon.StopAttack();
                }
            };
            
            RequestAddAction(1, castProjectileAction);

            //~ custom
            if (useWeapon)
            {
                if (trajectory is BoomerangTrajectory boomerangTrajectory)
                {
                    FlyWeapon flyWeapon = runtimeData.Weapon as FlyWeapon;
                    
                    boomerangTrajectory.OnChangePhase += phase =>
                    {
                        switch (phase)
                        {
                            case BoomerangTrajectory.Phase.Outbound:
                                flyWeapon.Startup();
                                break;
                            case BoomerangTrajectory.Phase.Hang:
                                projectile.StopLerpMotion();
                                flyWeapon.Phase01();
                                break;
                            case BoomerangTrajectory.Phase.Return:
                                projectile.StopLerpMotion();
                                flyWeapon.Phase02();
                                break;
                            case BoomerangTrajectory.Phase.Complete:
                                flyWeapon.Complete();
                                break;
                        }
                    };
                }
                else if (trajectory is SplineTrajectory splineTrajectory)
                {
                    FlyWeapon flyWeapon = runtimeData.Weapon as FlyWeapon;
                    
                    splineTrajectory.OnChangePhase += phase =>
                    {
                        switch (phase)
                        {
                            case SplineTrajectory.Phase.Windup:
                                flyWeapon.Startup();
                                break;
                            case SplineTrajectory.Phase.Execute:
                                projectile.StopLerpMotion();
                                flyWeapon.Phase01();
                                break;
                            case SplineTrajectory.Phase.Recovery:
                                projectile.StopLerpMotion();
                                flyWeapon.Phase02();
                                break;
                            case SplineTrajectory.Phase.Complete:
                                flyWeapon.Complete();
                                break;
                        }
                    };
                }
            }
            
            projectile.ImmediatelySetPosition(trajectory.EvaluatePosition(0));

            projectile.Initialize(runtimeData.Weapon);

            if(!useWeapon) projectile.Active();
        }
        
        /*
         * @Damage & Hp calculate
         */
        /// <summary>
        /// Phương thức này mục đích là gán HP entity đang bị ngắm & tính hp ngay khi bắn để tránh các vũ khí đều tranh vào 1 entity
        /// </summary>
        private void PredictedTargetDamage(SkillRuntimeData runtimeData, int entityTarget)
        {
            if (!EntityManager.IsEntityAlive(entityTarget)) return;
            
            runtimeData.CritChance = 0; // Lấy dmg gốc là được
            
            int damage = Mathf.CeilToInt(Formula.CalculateFinalDamage(runtimeData, out bool critical));

            ref HealthData healthData = ref ComponentManager<HealthData>.Get(entityTarget);

            healthData.PredictedHealth -= damage;

            MonoBehaviour pool = Pool.Instance;
            
            if (coroutinesResetFutureHealth.TryGetValue(entityTarget, out var coroutine))
            {
                pool.StopCoroutine(coroutine);
            }

            coroutinesResetFutureHealth[entityTarget] = pool.WaitInvoke(1, () =>
            {
                if (!EntityManager.IsEntityAlive(entityTarget)) return;
                
                ref HealthData healthData = ref ComponentManager<HealthData>.Get(entityTarget);
                healthData.PredictedHealth = healthData.CurrentHealth;
            });
        }
        
        private bool OnDamageEntityFunction(SkillData skillData, SkillRuntimeData runtimeData,
            DamageEntityInfo info, float scaleDamage, ref Action onProjectileDestroyed)
        {
            float radius = Mathf.Max(0, runtimeData.ExplosiveRadius);

            Vector3 position = Vector3.zero;
            int agent;

            bool valid = MonsterEntityManager.TryGetAgent(info.entity, out agent)
                         && AgentManager.TryGet_AgentPosition(agent, out position);

            if (!valid) return false;

            if (radius > 0)
            {
                
                SpawnExplosiveAura(runtimeData.ExplosivePrefabName, position);
                
#if UNITY_EDITOR
                GizmosLine.Circle(position, radius, Color.yellow, 0.1f);
#endif
                Vector2 size = Vector2.one * radius / 2f;
                
                float sqr = Mathf.Pow(radius, 2);

                float explosiveDamagePercent = runtimeData.ExplosiveDamagePercent;
                
                List<int> entities = query.GetAllEntities(position, size, EntityManager.IsEntityAlive);

                for (int i = 0; i < entities.Count; i++)
                {
                    int e = entities[i];

                    if (!MonsterEntityManager.TryGetAgent(e, out int a)) continue;

                    if (!AgentManager.TryGet_AgentPosition(a, out Vector3 p)) continue;

                    float d = Vector3.SqrMagnitude(p - position);

                    if (d < sqr)
                    {
                        float dmg = (e == info.entity ? 1 : explosiveDamagePercent) * scaleDamage;

                        Vector3 textDamagePosition = (skillData.trajectory == TrajectoryType.Stationary || !info.useProjectilePosition)
                            ? position
                            : info.projectilePosition;
                        
                        return CalculatorDamage(skillData, runtimeData,
                            info.entity, textDamagePosition, dmg, info.isLastCollision, ref onProjectileDestroyed
                        );
                    }
                }
            }
            else
            {
                Vector3 textDamagePosition = (skillData.trajectory == TrajectoryType.Stationary || !info.useProjectilePosition)
                    ? position
                    : info.projectilePosition;
                
                return CalculatorDamage(skillData, runtimeData,
                    info.entity, textDamagePosition, scaleDamage, info.isLastCollision, ref onProjectileDestroyed
                );
            }

            return false;
        }

        private bool CalculatorDamage(SkillData skillData, SkillRuntimeData runtimeData, 
            int entity, Vector3 textDamagePosition,
            float scaleDamage, bool lastCollision, ref Action onProjectileDestroyed)
        {
            if (!EntityManager.IsEntityAlive(entity)) return false;
            
            ref HealthData health = ref ComponentManager<HealthData>.Get(entity);
            
            if(health.CurrentHealth <= 0) return false;
            
            int damage = Mathf.CeilToInt(Formula.CalculateFinalDamage(runtimeData, out bool critical) * scaleDamage);
            
            health.CurrentHealth -= damage;
            
            int currentHealth = health.CurrentHealth;

            if (AgentManager.TryGet_Monster(entity, out Monster monster) && currentHealth > 0)
            {
                monster.BeHit();
            }

            OnPostDamage?.Invoke(new PostDamageParams
            {
                Damage = damage,
                Source = skillData
            });
                
            SpawnTextDamage(damage, critical, textDamagePosition);

            float killInstantBelow = runtimeData.ExecuteHealthPercent;
            
            float healthPercent = health.CurrentHealth / (float)health.MaxHealth;
            
            bool shouldKillInstantMonster = healthPercent <= killInstantBelow;

            if (healthPercent <= 0 || shouldKillInstantMonster)
            {
                if (ComponentManager<StatData>.TryGet(entity, out StatData data))
                {
                    OnPostEarnExp?.Invoke(new PostEarnExpParams
                    {
                        Exp = data.Exp
                    });
                }

                if (MonsterEntityManager.TryGetAgent(entity, out int agent))
                {
                    if (lastCollision)
                    {
                        AgentManager.Destroy_Agent(agent, ref onProjectileDestroyed);
                    }
                    else
                    {
                        AgentManager.Destroy_Agent(agent);                        
                    }
                }
            }

            return true;
        }

        /*
         * @Spawn objects
         */
        private async void SpawnExplosiveAura(string prefabName, Vector3 position)
        {
            if (string.IsNullOrEmpty(prefabName)) return;

            GameObject go = await AssetBundleManager.GetAssetCached<GameObject>(prefabName);

            if (names.Add(prefabName)) Pool.RegisterPool(go, true);

            GameObject o = Pool.Instantiate(go);
            o.transform.position = position;
        }

        private async void SpawnTextDamage(int damage, bool critical, Vector3 position)
        {
            GameObject go = await AssetBundleManager.GetAssetCached<GameObject>(critical
                    ? Path.TEXT_DAMAGE_CRITICAL
                    : Path.TEXT_DAMAGE_NORMAL);

            TextDamage ins = Pool.Instantiate(go).GetComponent<TextDamage>();
            float offsetX = RandomUtils.Range(-0.3f, 0.3f);
            ins.transform.position = position + new Vector3(offsetX, 0, 0);

            ins.Execute(damage);
        }

        /*
         * @Build element
         */
        private BaseCollider GetCollider(ColliderData colliderData, SkillData skillData)
        {
            switch (colliderData.type)
            {
                case ColliderType.Circle:
                    return new CircleCollider(query, colliderData.relativePosition,
                        skillData.collisionStartDelay, skillData.collisionDuration, colliderData.circleRadius
                    );
                case ColliderType.Rectangle:
                    return new RectangleCollider(query, colliderData.relativePosition,
                        skillData.collisionStartDelay, skillData.collisionDuration, colliderData.rectangleSize,
                        colliderData.dependencyRelativeRotation
                    );
                default:
                    Debug.LogError("Unknown Collider type " + colliderData.type);
                    return null;
            }
        }
        
        private BaseTrajectory GetTrajectory(SkillRuntimeData runtimeData, SkillData skillData, ref Vector3 position, 
            ref Vector3 destination, ref float duration)
        {
            float distance = 0;

            bool found = DistanceToCircleEdge(position, destination, skillData.attackRange + runtimeData.AttackRange,
                out distance, out Vector3 hitPoint
            );

            if (!found)
            {
                hitPoint = destination;
                
                distance = Vector3.Distance(position, destination);
            }

            TrajectoryData trajectoryData = runtimeData.Trajectory;
            
            switch (skillData.trajectory)
            {
                case TrajectoryType.Projectile:
                    
                    duration = skillData.projectileDuration;
                    
                    float speed = distance / duration;
                    
                    return new ProjectileTrajectory(trajectoryData.projectileCurve, speed,
                        duration, position, destination
                    );
                
                case TrajectoryType.Boomerang:
                    
                    duration = skillData.boomerangOutboundDuration + skillData.boomerangHangDuration +
                               skillData.boomerangReturnDuration;
                    
                    return new BoomerangTrajectory(trajectoryData.boomerangInitCurve,
                        trajectoryData.boomerangReturnCurve, skillData.boomerangOutboundDuration,
                        skillData.boomerangHangDuration, skillData.boomerangReturnDuration,
                        position, hitPoint);
                
                case TrajectoryType.Spline:

                    duration = skillData.splineWindupDuration
                               + skillData.splineExecuteDuration
                               + skillData.splineRecoveryDuration;
                    
                    return new SplineTrajectory(trajectoryData.spline.Spline,
                        skillData.splineWindupDuration, skillData.splineExecuteDuration,
                        skillData.splineRecoveryDuration,
                        position, destination
                    );
                
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
                            destination = RandomPositionInRadius(position, skillData.stationaryRandomRadius);
                            break;
                        case TrajectoryStationaryPivot.Weapon:
                            position = runtimeData.Pivot;
                            destination = runtimeData.Muzzle;
                            break;
                        default:
                            Debug.LogError("Undefined Trajectory Stationary Pivot");
                            break;
                    }

                    return new StationaryTrajectory(position, destination);
                
                default:
                    Debug.LogError("Unknown Trajectory type " + skillData.trajectory);
                    return null;
            }
        }
        
        private static Vector2 RandomPositionInRadius(Vector2 position, float radius)
        {
            return position + Random.insideUnitCircle * radius;
        }
        
        private static bool DistanceToCircleEdge(Vector3 position, Vector3 destination, float radius,
            out float distance, out Vector3 hitPoint)
        {
            Vector3 dir = (destination - position).normalized;

            // position = ray origin
            // center = Vector3.zero
            float b = Vector3.Dot(position, dir);
            float c = Vector3.Dot(position, position) - radius * radius;

            float delta = b * b - c;

            if (delta < 0f)
            {
                distance = -1f;
                hitPoint = Vector3.zero;
                return false;
            }

            distance = -b + Mathf.Sqrt(delta);
            hitPoint = position + dir * distance;
            return true;
        }
    }

    public struct PostDamageParams
    {
        public SkillData Source;
        public int Damage;
    }

    public struct PostEarnExpParams
    {
        public int Exp;
    }
}