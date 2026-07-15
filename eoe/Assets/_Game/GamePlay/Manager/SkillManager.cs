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

        public static void CastSkill(SkillData skillData, SkillRuntimeData runtimeData, Vector3 position, Vector3 destination, int entityTarget)
        {
            if (instance == null)
            {
                Debug.LogError("Instance SkillManager is null");
                return;
            }

            // ~todo: force cast skill if weapon can fly
            if (runtimeData.IsFlyWeapon)
            {
                instance.CastSkill_Private(skillData, runtimeData, position, destination, 1);
                
                return;
            }

            float size = skillData.size;

            instance.PredictedTargetDamage(runtimeData, entityTarget);
            
            Vector3 direction = (destination - position).normalized;
            
            bool extra = false;
            
            Vector3 perpendicular = new Vector3(-direction.y, direction.x, 0);
            
            float parallelSpacing = skillData.parallelDistanceStep;

            if (runtimeData.ParallelCount > 0)
            {
                int count = runtimeData.ParallelCount + 1;
                
                float scaleDamage = runtimeData.ParallelDamagePercent;
             
                for (int i = 0; i < count; i++)
                {
                    float offset = (i - (count - 1) * 0.5f) * parallelSpacing * size;

                    Vector3 offsetPos = position + perpendicular * offset;
                    
                    Vector3 offsetDest = destination + perpendicular * offset;

                    instance.CastSkill_Private(skillData, runtimeData, offsetPos, offsetDest, scaleDamage);
                }
                    
                extra = true;
            }

            if (runtimeData.SpreadCount > 0)
            {
                int count = runtimeData.SpreadCount + 1;
                
                float angleStep = skillData.spreadAngleStep;
                
                float scaleDamage = runtimeData.SpreadDamagePercent;

                float d = Mathf.CeilToInt(runtimeData.ParallelCount / 2f) * parallelSpacing;
                
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
        
        private async void CastSkill_Private(SkillData skillData, SkillRuntimeData runtimeData,
            Vector3 position, Vector3 destination, float scaleDamage)
        {
            float lifeTime = 0;
       
            BaseTrajectory trajectory = GetTrajectory(runtimeData.Trajectory, skillData, position, destination, ref lifeTime);
            if (trajectory == null)
            {
#if UNITY_EDITOR
                Debug.LogError("Stop cast skill because Trajectory is null, skillId " + skillData.skillId);
#endif
                return;
            }
            
            Projectile projectile = null;
            BaseCollider collider = null;
            
            bool isFlyWeapon = runtimeData.IsFlyWeapon;
          
            if (isFlyWeapon)
            {
                projectile = runtimeData.FlyWeapon.GetComponent<Projectile>();
                if (projectile == null)
                {
#if UNITY_EDITOR
                    Debug.LogError("Stop cast skill because Projectile Component is null, prefab " + runtimeData.FlyWeapon.name);
#endif
                    return;
                }
               
                projectile.SetSizeScale(skillData.size);
                collider = GetCollider(projectile.ColliderData, skillData);
                if (collider == null)
                {
#if UNITY_EDITOR
                    Debug.LogError("Stop cast skill because Collider is null, skillId " + skillData.skillId);
#endif
                    return;
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
                
                projectile.SetSizeScale(skillData.size);
                collider = GetCollider(projectile.ColliderData, skillData);
                if (collider == null)
                {
#if UNITY_EDITOR
                    Debug.LogError("Stop cast skill because Collider is null, skillId " + skillData.skillId);
#endif
                    return;
                }
             
                GameObject go = Pool.Instantiate(prefab, position, false);
                projectile = go.GetComponent<Projectile>();
            }
            
            Action onProjectileDestroyed = () => { };
            CastProjectileAction castProjectileAction = new CastProjectileAction(this, lifeTime, collider, trajectory,
                (entity, pos, lastCollision) =>
                    OnDamageEntityFunction(skillData, runtimeData, entity, pos, scaleDamage, lastCollision,
                        ref onProjectileDestroyed),
                projectile, skillData.damageInterval,
                skillData.collLimitCollision + runtimeData.PiercingCount, skillData.collResetCollision
            );
            
            castProjectileAction.OnComplete += () =>
            {
                if (isFlyWeapon) runtimeData.FlyWeapon.OnStopAttack();

                projectile.Destroy(onProjectileDestroyed);         
            };
            
            RequestAddAction(1, castProjectileAction);

            //~ custom
            if (isFlyWeapon)
            {
                if (trajectory is BoomerangTrajectory boomerangTrajectory)
                {
                    FlyWeapon flyWeapon = runtimeData.FlyWeapon as FlyWeapon;
                    
                    boomerangTrajectory.OnChangePhase += phase =>
                    {
                        switch (phase)
                        {
                            case BoomerangTrajectory.Phase.Outbound:
                                flyWeapon.OutboundFly();
                                break;
                            case BoomerangTrajectory.Phase.Hang:
                                flyWeapon.HangFly();
                                break;
                            case BoomerangTrajectory.Phase.Return:
                                flyWeapon.ReturnFly();
                                projectile.DisableTrail();
                                break;
                            case BoomerangTrajectory.Phase.Complete:
                                flyWeapon.CompleteFly();
                                break;
                        }
                    };
                }
            }

            projectile.Initialize();

            if(!isFlyWeapon) projectile.gameObject.SetActive(true);
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
            int entity, Vector3 position, float scaleDamage, bool lastCollision, ref Action onProjectileDestroyed)
        {
            float radius = Mathf.Max(0, runtimeData.ExplosiveRadius);

            if (radius > 0)
            {
#if UNITY_EDITOR
                GizmosLine.Circle(position, radius, Color.yellow, 0.1f);
#endif
                Vector2 size = new Vector2(radius / 2f, radius / 2f);
                float sqrRadius = radius * radius;
                float explosiveDamage = runtimeData.ExplosiveDamagePercent;

                List<int> entities = query.GetAllEntities(position, size, EntityManager.IsEntityAlive);
                for (int i = entities.Count - 1; i >= 0; i--)
                {
                    int e = entities[i];

                    if (!MonsterEntityManager.TryGetAgent(e, out int agent)) continue;
                    
                    if (AgentManager.TryGet_AgentPosition(agent, out Vector3 entityPosition))
                    {
                        float d = Vector3.SqrMagnitude(entityPosition - position);

                        if (d < sqrRadius)
                        {
                            float dmg = (e == entity ? 1.0f : explosiveDamage) * scaleDamage;

                            return CalculatorDamage(skillData, runtimeData,
                                entity, position, dmg, lastCollision, ref onProjectileDestroyed
                            );
                        }
                    }
                }

                SpawnExplosiveAura(skillData.explosivePrefabName, radius, position);
            }
            else
            {
#if UNITY_EDITOR
                GizmosLine.Line(position - new Vector3(0.25f, 0.25f),
                    position + new Vector3(0.25f, 0.25f),
                    Color.yellow, 0.1f
                );
                GizmosLine.Line(position + new Vector3(-0.25f, 0.25f),
                    position + new Vector3(0.25f, -0.25f),
                    Color.yellow, 0.1f
                );
#endif
                return CalculatorDamage(skillData, runtimeData, entity, position, scaleDamage,
                    lastCollision, ref onProjectileDestroyed
                );
            }

            return true;
        }

        private bool CalculatorDamage(SkillData skillData, SkillRuntimeData runtimeData, 
            int entity, Vector3 position,
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
                
            SpawnTextDamage(damage, critical, position);

            float killInstantBelow = runtimeData.KillInstantBelowHealthPercent;
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
        private async void SpawnExplosiveAura(string prefabName, float radius, Vector3 position)
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
                        skillData.collTimerTrigger, skillData.collDuration, colliderData.circleRadius
                    );
                default:
                    Debug.LogError("Unknown Collider type " + colliderData.type);
                    return null;
            }
        }
        
        private BaseTrajectory GetTrajectory(TrajectoryData trajectoryData, SkillData skillData, Vector3 position, Vector3 destination, ref float duration)
        {
            float d = Vector3.Distance(position, Vector3.zero);
            switch (skillData.trajectory)
            {
                case TrajectoryType.Projectile:
                    duration = (skillData.findRadius - d) / skillData.projectileSpeed;
                    return new ProjectileTrajectory(trajectoryData.projectileCurve, skillData.projectileSpeed,
                        duration, position, destination
                    );
                case TrajectoryType.Boomerang:
                    duration = skillData.boomerangOutboundDuration + skillData.boomerangHangDuration +
                               skillData.boomerangReturnDuration;
                    return new BoomerangTrajectory(trajectoryData.boomerangInitCurve,
                        trajectoryData.boomerangReturnCurve,
                        skillData.boomerangOutboundSpeed, skillData.boomerangOutboundDuration,
                        skillData.boomerangHangDuration, skillData.boomerangReturnDuration,
                        position, destination);
                default:
                    Debug.LogError("Unknown Trajectory type " + trajectoryData.type);
                    return null;
            }
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