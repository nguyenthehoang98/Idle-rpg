using System;
using System.Collections.Generic;
using _KITSystem.Data;
using _KITSystem.Resource;
using _KITSystem.SkillSystem.Config;
using _KITSystem.SkillSystem.Entity;
using _KITSystem.Utils;
using Unity.Mathematics;
using UnityEngine;

namespace _KITSystem.SkillSystem.Runtime
{
    internal class CastProjectileSkillAction : BaseSkillAction
    {
        private DamageTicket damageTicket;
        private float collisionResetIntervalInSeconds;
        
        private BaseShapeAction shape;
        private BaseTrajectoryAction trajectory;
        private Transform projectile;

        private HashSet<int> hashset = new HashSet<int>();
        private int remainingCollisions;
        private float collisionResetElapsedTime;

        private Dictionary<int, float> targetDamageTicketElapsedTimes = new Dictionary<int, float>();
        private List<int> targetDamageTicketEntities = new List<int>();
        private float casterDamageTicketElapsedTime;

        private float2 previousPosition;
        private float2 direction;
        
        public CastProjectileSkillAction(
            CastProjectileConfig config,
            BaseTrajectoryAction trajectory, BaseShapeAction shape, Transform projectile,
            SPU spu, TriggerConfig triggerConfig, float lifeTime) :
            base(spu, triggerConfig, lifeTime)
        {
            this.damageTicket = config.damageTicket;
            this.trajectory = trajectory;
            this.shape = shape;
            this.projectile = projectile;
            this.collisionResetIntervalInSeconds = config.collisionResetIntervalInSeconds;
            this.remainingCollisions = config.maximumCollision;
            
            if (damageTicket.damageTickerType == DamageTickerType.CasterInterval)
            {
                casterDamageTicketElapsedTime = damageTicket.damageTickerIntervalInSeconds;
            }
        }

        protected override void OnUpdate(float deltaTime)
        {
            // todo: reset collision
            
            collisionResetElapsedTime += deltaTime;
            
            if (collisionResetElapsedTime >= collisionResetIntervalInSeconds && hashset.Count > 0)
            {
                hashset.Clear();
                collisionResetElapsedTime = 0f;
            }
            
            // todo: update dmg
            
            casterDamageTicketElapsedTime += deltaTime;
            
            UpdateTargetDamageTickets(deltaTime);
            
            // update position
            float2 position = trajectory.EvaluatePosition(deltaTime);
            
            direction = MathUtils.NormalizeSafe(position - previousPosition);
            
            previousPosition = position;
            
            projectile.transform.position = new Vector3(position.x, position.y);
            
            // scan collider
            shape.Tick(deltaTime);
            
            List<int> results = shape.Hit(position);
            
            if (results != null)
            {
                bool hit = results.Count > 0;
#if UNITY_EDITOR
                shape.Gizmos(new Vector3(position.x, position.y), hit ? Color.red : Color.green, 0.1f);
#endif
                if (hit)
                {
                    for (int i1 = 0; i1 < results.Count; i1++)
                    {
                        int entity = results[i1];

                        if (CanProcessCollision(entity))
                        {
                            if (!TryDamage(entity))
                            {
                                continue;
                            }

                            remainingCollisions--;
                            
                            if (remainingCollisions == 0)
                            {
                                EndLifeCycle();
                                
                                return;
                            }
                        }
                    }
                }
            }
        }
        
        protected override void OnStop()
        {
            base.OnStop();

            if (projectile != null)
            {
                KitPool.Destroy(projectile.gameObject);

                projectile = null;
            }
        }
        
        private bool TryDamage(int entity)
        {
            switch (damageTicket.damageTickerType)
            {
                case DamageTickerType.CasterInterval:
                    if (casterDamageTicketElapsedTime < damageTicket.damageTickerIntervalInSeconds)
                    {
                        return false;
                    }

                    if (Damage(entity, damageTicket))
                    {
                        casterDamageTicketElapsedTime = 0f;
                        return true;
                    }

                    return false;
                case DamageTickerType.TargetInterval:
                    if (targetDamageTicketElapsedTimes.TryGetValue(entity, out float elapsedTime)
                        && elapsedTime < damageTicket.damageTickerIntervalInSeconds)
                    {
                        return false;
                    }

                    if (Damage(entity, damageTicket))
                    {
                        targetDamageTicketElapsedTimes[entity] = 0f;
                        return true;
                    }

                    return false;
                default:
                    return Damage(entity, damageTicket);
            }
        }
        
        private bool CanProcessCollision(int entity)
        {
            if (damageTicket.damageTickerType != DamageTickerType.None)
            {
                return true;
            }

            return hashset.Add(entity);
        }

        protected bool Damage(int entity, DamageTicket damageTicket)
        {
            if (!EntityManager.IsAlive(entity) || !ComponentManager<HealthData>.Has(entity))
            {
                return false;
            }

            ref var health = ref ComponentManager<HealthData>.Get(entity);
            int dmg = DamageOutput(entity);

            if (damageTicket != null && damageTicket.isHpPercent)
            {
                dmg = (int)Math.Ceiling(health.MaxHealth * dmg / 100f);
            }

            if (dmg <= 0)
            {
                return false;
            }

            health.CurrentHealth -= dmg;

            OnDamageEffect(entity);
            
            if (health.CurrentHealth <= 0)
            {
                EntityManager.DestroyEntity(entity);
            }

            return true;
        }

        private void UpdateTargetDamageTickets(float deltaTime)
        {
            if (damageTicket.damageTickerType != DamageTickerType.TargetInterval)
            {
                return;
            }

            targetDamageTicketEntities.Clear();
            foreach (var pair in targetDamageTicketElapsedTimes)
            {
                targetDamageTicketEntities.Add(pair.Key);
            }

            for (int i = 0; i < targetDamageTicketEntities.Count; i++)
            {
                int entity = targetDamageTicketEntities[i];
                targetDamageTicketElapsedTimes[entity] += deltaTime;
            }
        }

        private void OnDamageEffect(int entity)
        {
            EntityManager.InvokeBehaviour(entity, EntityManagerBehaviourType.BeHit, ParameterValue.Vector3(new Vector3(direction.x, direction.y, 0f)));
        }

        private int DamageOutput(int entity) => 10;
    }
}
