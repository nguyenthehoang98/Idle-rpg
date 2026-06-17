using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using _BattleSource.Entity;
using _Game.Configs;
using _KITSystem.Entity;
using _KITSystem.Resource;
using _KITSystem.Schedule;
using _KITSystem.SkillSystem.Core;
using _KITSystem.SkillSystem.Imp;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Game.GamePlay.SkillSystem
{
    [Serializable]
    public class SkillTickable : Spu, ITickable
    {
        private IQuery query = new EntityQuery();

        private HashSet<GameObject> projectilesObject = new HashSet<GameObject>();
        private HashSet<string> projectilesName = new HashSet<string>();

        public event Action<SkillData, int> OnPostDamage; 
        
        private static SkillTickable instance;
        
        public Task Initialize()
        {
            instance = this;
            
            return Task.CompletedTask;
        }

        public static void CastSkill(SkillData skillData, Vector3 position, Vector3 destination)
        {
            if (instance != null)
            {
                Vector3 direction = (destination - position).normalized;
                
                bool extra = false;

                if (skillData.extra.parallelProjectileCount > 0)
                {
                    int count = skillData.extra.parallelProjectileCount + 1;
                    float spacing = skillData.extra.parallelDistanceStep;
                    Vector3 perpendicular = new Vector3(-direction.y, direction.x, 0);
                    
                    for (int i = 0; i < count; i++)
                    {
                        float offset = (i - (count - 1) * 0.5f) * spacing;

                        Vector3 offsetPos = position + perpendicular * offset;
                        Vector3 offsetDest = destination + perpendicular * offset;

                        instance.CastSkillPrivate(skillData, offsetPos, offsetDest);
                    }
                    
                    extra = true;
                }

                if (skillData.extra.spreadProjectileCount > 0)
                {
                    int count = skillData.extra.spreadProjectileCount + 1;
                    float angleStep = skillData.extra.spreadAngleStep;
                    
                    for (int i = 0; i < count; i++)
                    {
                        float angle = (i - (count - 1) * 0.5f) * angleStep / Mathf.Max(1, count - 1);

                        Vector3 dir = Quaternion.Euler(0, 0, angle) * direction;

                        instance.CastSkillPrivate(skillData, position, position + dir * 100f);
                    }
                    
                    extra = true;
                }
                
                if(!extra) instance.CastSkillPrivate(skillData, position, destination);
            }
            else
                Debug.LogError("Instance AnimationTickable is null");
        }

        public static void FindTarget(FindTargetType type, Vector2 center, float radius,
            Func<int, bool> funcFilterEntity, out QueryResult result)
        {
            instance.query.FindTarget(type, center, radius, funcFilterEntity, out result);
        }

        async void CastSkillPrivate(SkillData skillData, Vector3 position, Vector3 destination)
        {
            ColliderData colliderData = skillData.collider;
            BaseCollider collider = new CircleCollider(
                query, colliderData.relativePosition, colliderData.timerTrigger,
                colliderData.duration, colliderData.radius
            );
            TrajectoryData trajectoryData = skillData.trajectory;
            BaseTrajectory trajectory = new BulletTrajectory(trajectoryData.bulletInitSpeed,
                trajectoryData.bulletAcceleration, position, destination
            );
            GameObject go = null;
            if (!string.IsNullOrEmpty(skillData.prefabName))
            {
                go = await AssetBundleManager.GetAssetCached<GameObject>(skillData.prefabName);
                if (projectilesName.Add(skillData.prefabName))
                {
                    Pool.RegisterPool(go, true);
                    projectilesObject.Add(go);
                }

                go = Pool.Instantiate(go);
                go.transform.position = position;
            }

            DamageTickerData damageTicket = skillData.damageTicker;
            _KITSystem.SkillSystem.Core.DamageTickerType dtt = Enum.Parse<_KITSystem.SkillSystem.Core.DamageTickerType>(
                damageTicket.type.ToString()
            );
            CastProjectileAction action = new CastProjectileAction(this, skillData.lifeTime, collider, trajectory,
                (entity, pos) => DamageEntity(skillData, pos, entity), go, dtt, damageTicket.ticketInterval,
                colliderData.limitNumberCollision, colliderData.resetCollisionInterval
            );

            if (go != null)
            {
                Projectile p = go.GetComponent<Projectile>();
                if (p != null)
                    p.Initialize();
                else Debug.LogError("Projectile is null");
            }
            else Debug.LogError("Projectile is null");

            action.OnComplete += () =>
            {
                if (go != null)
                {
                    Projectile p = go.GetComponent<Projectile>();
                    if (p != null) p.Destroy();
                }
            };

            RequestAddAction(1, action);
        }

        private bool DamageEntity(SkillData skillData, Vector2 position, int entity)
        {
            if (skillData.extra.isExplosive)
            {
                Vector2 size = new Vector2(skillData.extra.explosiveRadius, skillData.extra.explosiveRadius);

#if UNITY_EDITOR
                Gizmos(position, skillData.extra.explosiveRadius * 2, Color.yellow, 1 / 30f);
#endif

                SpawnAura(skillData.extra.explosivePrefabName, position, skillData.extra.explosiveRadius);
              
                List<int> entities = query.GetAllEntities(position, size, EntityManager.IsEntityAlive);

                foreach (var e in entities)
                {
                    ref HealthData health = ref ComponentManager<HealthData>.Get(e);

                    int damage = 50;

                    health.CurrentHealth -= damage;

                    if (AgentTickable.TryGetMonster(e, out Monster monster))
                    {
                        monster.BeBit();
                    }
            
                    OnPostDamage?.Invoke(skillData, damage);
            
                    if (health.CurrentHealth <= 0) AgentTickable.Remove(e);
                }
            }
            else
            {
                bool alive = EntityManager.IsEntityAlive(entity);

                if (!alive) return false;

                ref HealthData health = ref ComponentManager<HealthData>.Get(entity);

                int damage = 50;
            
                health.CurrentHealth -= damage;

                if (AgentTickable.TryGetMonster(entity, out Monster monster))
                {
                    monster.BeBit();
                }
            
                OnPostDamage?.Invoke(skillData, damage);
            
                if (health.CurrentHealth <= 0) AgentTickable.Remove(entity);
            }
            
            return true;
        }

        private async void SpawnAura(string prefabName, Vector3 position, float radius)
        {
            if (string.IsNullOrEmpty(prefabName)) return;
            
            GameObject go = await AssetBundleManager.GetAssetCached<GameObject>(prefabName);

            if (projectilesName.Add(prefabName))
            {
                Pool.RegisterPool(go, true);
            }
            
            GameObject o = Pool.Instantiate(go);
            o.transform.position = position;
            
            Aura aura = o.GetComponent<Aura>();
            aura.Scale(radius);
        }
        
        private void Gizmos(Vector3 position, float radius, Color color, float deltaTime)
        {
            Vector2 center = position;
            int segments = 12;
            float angleStep = 360f / segments;
            Vector2 prevPoint = center + new Vector2(Mathf.Cos(0f), Mathf.Sin(0f)) * radius;
            for (int i = 1; i <= segments; i++)
            {
                float angle = angleStep * i;
                Vector2 newPoint = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                Debug.DrawLine(new Vector3(prevPoint.x, prevPoint.y), new Vector3(newPoint.x, newPoint.y), color, deltaTime);
                prevPoint = newPoint;
            }
        }
        
        public override void Dispose()
        {
            base.Dispose();

            foreach (var name in projectilesName)
            {
                AssetBundleManager.UnCache(name);
            }

            projectilesName = null;

            foreach (var go in projectilesObject)
            {
                Pool.UnRegisterPool(go);
            }

            projectilesObject = null;

            instance = null;
        }
    }
}
