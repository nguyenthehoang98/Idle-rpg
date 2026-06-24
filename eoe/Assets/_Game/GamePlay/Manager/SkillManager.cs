using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using _BattleSource.Entity;
using _Game._GamePlay;
using _Game.Configs;
using _KITSystem.Entity;
using _KITSystem.Grid;
using _KITSystem.Resource;
using _KITSystem.Schedule;
using _KITSystem.SkillSystem.Core;
using _KITSystem.SkillSystem.Imp;
using UnityEngine;

namespace _Game.GamePlay.Manager
{
    [Serializable]
    public sealed class SkillManager : Spu, ITickable
    {
        static SkillManager instance;

        public event Action<PostDamageParams> OnPostDamage;
        public event Action<PostEarnExpParams> OnPostEarnExp;

        private IQuery query;
        private HashSet<GameObject> objects;
        private HashSet<string> names;

        public Task Initialize()
        {
            query = new EntityQuery();
            objects = new HashSet<GameObject>();
            names = new HashSet<string>();

            instance = this;
            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            base.Dispose();

            foreach (var go in objects)
            {
                Pool.UnRegisterPool(go);
            }

            objects = null;

            foreach (var name in names)
            {
                AssetBundleManager.UnCache(name);
            }

            names = null;

            instance = null;
        }

        public static void CastSkill(SkillData skillData, SkillRuntimeData runtimeData, Vector3 position,
            Vector3 destination)
        {
            if (instance == null)
            {
                Debug.LogError("Instance SkillManager is null");
                return;
            }
            
            Vector3 direction = (destination - position).normalized;
            
            bool extra = false;

            if (runtimeData.ParallelCount > 0)
            {
                int count = runtimeData.ParallelCount + 1;
                float spacing = skillData.extra.parallelDistanceStep;
                float scaleDamage = runtimeData.ParallelDamagePercent;
                Vector3 perpendicular = new Vector3(-direction.y, direction.x, 0);
             
                int mid = count / 2;
                
                for (int i = 0; i < count; i++)
                {
                    float offset = (i - (count - 1) * 0.5f) * spacing;

                    Vector3 offsetPos = position + perpendicular * offset;
                    Vector3 offsetDest = destination + perpendicular * offset;

                    instance.CastSkill_Private(skillData, runtimeData, offsetPos, offsetDest, mid == i ? 1 : scaleDamage);
                }
                    
                extra = true;
            }

            if (runtimeData.SpreadCount > 0)
            {
                int count = runtimeData.SpreadCount + 1;
                float angleStep = skillData.extra.spreadAngleStep;
                float scaleDamage = runtimeData.SpreadDamagePercent;
                
                int mid = count / 2;
                
                for (int i = 0; i < count; i++)
                {
                    float angle = (i - (count - 1) * 0.5f) * angleStep / Mathf.Max(1, count - 1);
                    Vector3 dir = Quaternion.Euler(0, 0, angle) * direction;
                    instance.CastSkill_Private(skillData, runtimeData, position, position + dir * 100f,  mid == i ? 1 : scaleDamage);
                }
                    
                extra = true;
            }
                
            if(!extra) instance.CastSkill_Private(skillData, runtimeData, position, destination, 1);
        }
        
        private async void CastSkill_Private(SkillData skillData, SkillRuntimeData runtimeData,
            Vector3 position, Vector3 destination, float scaleDamage)
        {
            ColliderData colliderData = skillData.collider;
            BaseCollider collider = null;
            switch (colliderData.type)
            {
                case ColliderType.Circle:
                    collider = new CircleCollider(
                        query, colliderData.relativePosition, colliderData.timerTrigger,
                        colliderData.duration, colliderData.radius
                    );
                    break;
                default:
                    Debug.LogError($"Collider Type not supported {colliderData.type}");
                    break;
            }

            TrajectoryData trajectoryData = skillData.trajectory;
            BaseTrajectory trajectory = null;
            switch (trajectoryData.type)
            {
                case TrajectoryType.Bullet:
                    trajectory = new BulletTrajectory(trajectoryData.bulletInitSpeed,
                        trajectoryData.bulletAcceleration, position, destination
                    );
                    break;
                default:
                    Debug.LogError($"Trajectory Type not supported {trajectoryData.type}");
                    break;
            }

            GameObject go = await AssetBundleManager.GetAssetCached<GameObject>(skillData.prefabName);
            if (names.Add(skillData.prefabName) && objects.Add(go))
                Pool.RegisterPool(go, true);

            go = Pool.Instantiate(go);
            go.transform.position = position;

            Projectile projectile = go.GetComponent<Projectile>();
            if (projectile == null) Debug.LogError($"Projectile Component is null at '{go.name}'");

            DamageTickerData damageTicket = skillData.damageTicker;
            _KITSystem.SkillSystem.Core.DamageTickerType dtt = Enum.Parse<_KITSystem.SkillSystem.Core.DamageTickerType>(
                damageTicket.type.ToString()
            );

            CastProjectileAction action = new CastProjectileAction(this, skillData.lifeTime, collider, trajectory,
                (entity, pos) => OnDamageEntityFunction(skillData, runtimeData, entity, position, scaleDamage),
                projectile, dtt, damageTicket.ticketInterval,
                colliderData.limitNumberCollision, colliderData.resetCollisionInterval
            );
            action.OnComplete += () => { projectile.Destroy(); };

            projectile.Initialize();

            RequestAddAction(1, action);
        }

        private bool OnDamageEntityFunction(SkillData skillData, SkillRuntimeData runtimeData, int entity,
            Vector3 position, float scaleDamage)
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
                            
                            return CalculatorDamage(skillData, runtimeData, entity, position, dmg);
                        }
                    }
                }

                SpawnExplosiveAura(skillData.extra.explosivePrefabName, radius, position);
            }
            else
            {
                return CalculatorDamage(skillData, runtimeData, entity, position, scaleDamage);
            }

            return true;
        }

        private bool CalculatorDamage(SkillData skillData, SkillRuntimeData runtimeData, int entity, Vector3 position,
            float scaleDamage)
        {
            if (!EntityManager.IsEntityAlive(entity)) return false;

            int damage = Mathf.CeilToInt(Formula.CalculateFinalDamage(runtimeData, out bool critical) * scaleDamage);

            ref HealthData health = ref ComponentManager<HealthData>.Get(entity);
            health.CurrentHealth -= damage;

            if (AgentManager.TryGet_Monster(entity, out Monster monster) && health.CurrentHealth > 0) monster.BeHit();

            OnPostDamage?.Invoke(new PostDamageParams
            {
                Damage = damage,
                Source = skillData
            });

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
                    AgentManager.Destroy_Agent(agent);
                }
            }

            SpawnTextDamage(damage, critical, position);

            return true;
        }

        private async void SpawnExplosiveAura(string prefabName, float radius, Vector3 position)
        {
            if (string.IsNullOrEmpty(prefabName)) return;

            GameObject go = await AssetBundleManager.GetAssetCached<GameObject>(prefabName);

            if (names.Add(prefabName) && objects.Add(go))
            {
                Pool.RegisterPool(go, true);
            }

            GameObject o = Pool.Instantiate(go);
            o.transform.position = position;

            o.GetComponent<Aura>().Scale(radius);
        }

        private async void SpawnTextDamage(int damage, bool critical, Vector3 position)
        {
            GameObject go =
                await AssetBundleManager.GetAssetCached<GameObject>(critical
                    ? Const.TEXT_DAMAGE_CRITICAL
                    : Const.TEXT_DAMAGE_NORMAL);

            TextDamage ins = Pool.Instantiate(go).GetComponent<TextDamage>();
            ins.transform.position = position;

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