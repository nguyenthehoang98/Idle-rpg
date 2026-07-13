using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using _Game.Configs;
using _Game.Entry;
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
using DamageTickerType = _Game.Configs.DamageTickerType;

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

            instance.SyncFutureDamage(runtimeData, entityTarget);
            
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
                    float offset = (i - (count - 1) * 0.5f) * parallelSpacing;

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

                float d = (Mathf.CeilToInt(runtimeData.ParallelCount / 2f) * parallelSpacing);
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

        private void SyncFutureDamage(SkillRuntimeData runtimeData, int entityTarget)
        {
            if (!EntityManager.IsEntityAlive(entityTarget)) return;
            
            runtimeData.CritChance = 0; // Lấy dmg gốc là được
            
            int damage = Mathf.CeilToInt(Formula.CalculateFinalDamage(runtimeData, out bool critical));

            ref HealthData healthData = ref ComponentManager<HealthData>.Get(entityTarget);

            healthData.FutureHealth -= damage;

            MonoBehaviour pool = Pool.Instance;
            
            if (coroutinesResetFutureHealth.TryGetValue(entityTarget, out var coroutine))
            {
                pool.StopCoroutine(coroutine);
            }

            coroutinesResetFutureHealth[entityTarget] = pool.WaitInvoke(1, () =>
            {
                if (!EntityManager.IsEntityAlive(entityTarget)) return;
                
                ref HealthData healthData = ref ComponentManager<HealthData>.Get(entityTarget);
                healthData.FutureHealth = healthData.CurrentHealth;
            });
        }
        
        private async void CastSkill_Private(SkillData skillData, SkillRuntimeData runtimeData,
            Vector3 position, Vector3 destination, float scaleDamage)
        {
            BaseTrajectory trajectory = null;
            float lifetimeProjectile = 0;
            float distanceDelta = Vector3.Distance(position, Vector3.zero);
            
            switch (skillData.trajectoryType)
            {
                case TrajectoryType.Bullet:
                    // s=0.5at2 + vt
                    float s = skillData.filterRadius - distanceDelta;
                    float v = skillData.bulletInitSpeed;
                    float a = skillData.bulletAcceleration;

                    if (a > 0)
                    {
                        float dt = v * v + 2f * a * s;
                        if (dt >= 0) lifetimeProjectile = (-v + Mathf.Sqrt(dt)) / a;
                    }
                    else
                    {
                        lifetimeProjectile = s / v;
                    }
                    
                    trajectory = new BulletTrajectory(v, a, position, destination);
                    break;
                default:
                    Debug.LogError($"Trajectory Type not supported {skillData.trajectoryType}");
                    break;
            }

            GameObject go = await AssetBundleManager.GetAssetCached<GameObject>(skillData.prefabName);
            go = Pool.Instantiate(go, false);
            go.transform.position = position;

            Projectile projectile = go.GetComponent<Projectile>();
            if (projectile == null) Debug.LogError($"Projectile Component is null at '{go.name}'");
            
            BaseCollider collider = null;
            switch (projectile.ShapeType)
            {
                case ColliderType.Circle:
                    collider = new CircleCollider(
                        query, Vector2.zero, skillData.collTimerTrigger, skillData.collDuration, projectile.CircleRadius
                    );
                    break;
                default:
                    Debug.LogError($"Collider Type not supported {projectile.ShapeType}");
                    break;
            }

            DamageTickerType tickerType = skillData.tickerType;
            _KITSystem.SkillSystem.Core.DamageTickerType dtt = Enum.Parse<_KITSystem.SkillSystem.Core.DamageTickerType>(
                tickerType.ToString()
            );

            if (lifetimeProjectile <= 0) lifetimeProjectile = skillData.lifeTime;
            
            Action onProjectileDestroyed = () => { };
            CastProjectileAction action = new CastProjectileAction(this, lifetimeProjectile, collider, trajectory,
                (entity, pos, lastCollision) =>
                    OnDamageEntityFunction(skillData, runtimeData, entity, pos, scaleDamage, lastCollision,
                        ref onProjectileDestroyed),
                projectile, dtt, skillData.ticketInterval,
                skillData.collLimitCollision + runtimeData.PiercingCount, skillData.collResetCollision
            );
            
            action.OnComplete += () =>
            {
                /*onProjectileDestroyed += () =>
                {
                    if (action.Reason == ActionCompleteReason.Interrupt)
                        Pool.Instantiate(runtimeData.ImpactEffectPrefab, true).transform.position = projectile.TargetPosition;
                };*/

                projectile.Destroy(onProjectileDestroyed);
            };

            RequestAddAction(1, action);

            projectile.Initialize();
            
            projectile.gameObject.SetActive(true);
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

        private async void SpawnExplosiveAura(string prefabName, float radius, Vector3 position)
        {
            if (string.IsNullOrEmpty(prefabName)) return;

            GameObject go = await AssetBundleManager.GetAssetCached<GameObject>(prefabName);

            if (names.Add(prefabName)) Pool.RegisterPool(go, true);

            GameObject o = Pool.Instantiate(go);
            o.transform.position = position;

            //o.GetComponent<Aura>().Scale(radius);
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