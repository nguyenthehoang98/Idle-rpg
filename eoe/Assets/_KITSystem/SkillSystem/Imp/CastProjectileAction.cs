using System;
using System.Collections.Generic;
using _KITSystem.SkillSystem.Core;
using UnityEngine;

namespace _KITSystem.SkillSystem.Imp
{
    public class CastProjectileAction : BaseAction
    {
        private readonly BaseCollider collider;
        private readonly BaseTrajectory trajectory;
        private readonly DamageTickerType damageTickerType;
        private readonly float damageTickerInterval;
        private readonly int limitNumberCollisions;
        private readonly float resetCollisionInterval;
        private readonly Func<int, Vector2, bool> onDamageEntity;

        public event Action OnComplete;

        private Vector2 previousPosition;
        private GameObject projectile;
        private HashSet<int> collisions;
        private int totalCollisions;
        private float collisionResetElapsedTime;

        private float damageTickerElapsedTime;

        public CastProjectileAction(Spu spu, float lifeTime, BaseCollider collider, BaseTrajectory trajectory,
            Func<int, Vector2, bool> onDamageEntity,
            GameObject projectile, DamageTickerType damageTickerType, float damageTickerInterval,
            int limitNumberCollisions, float resetCollisionInterval) : base(spu, lifeTime)
        {
            this.onDamageEntity = onDamageEntity;
            this.collider = collider;
            this.trajectory = trajectory;
            this.projectile = projectile;
            this.damageTickerType = damageTickerType;
            this.limitNumberCollisions = limitNumberCollisions;
            this.resetCollisionInterval = resetCollisionInterval > 0 ? resetCollisionInterval : float.MaxValue;
            this.damageTickerInterval = damageTickerInterval;
            this.collisions = new HashSet<int>();
            this.collisionResetElapsedTime = this.damageTickerElapsedTime = 0;
            this.totalCollisions = 0;
            this.previousPosition = projectile.transform.position;
        }

        protected override void OnUpdate(float deltaTime)
        {
            this.collisionResetElapsedTime += deltaTime;

            if (this.collisionResetElapsedTime >= this.resetCollisionInterval && this.collisions.Count > 0)
            {
                this.collisions.Clear();
                this.collisionResetElapsedTime = 0;
            }

            damageTickerElapsedTime += deltaTime;

            Vector2 position = this.trajectory.EvaluatePosition(deltaTime);
            Vector2 direction = position - previousPosition;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            
            projectile.transform.position = position;
            projectile.transform.rotation =  Quaternion.Euler(0, 0, angle + 90f);
            
            previousPosition = position;

            collider.Tick(deltaTime);

            List<int> results = collider.Collision(position);

            bool hit = results != null && results.Count > 0;

#if UNITY_EDITOR
            collider.Gizmos(position, hit ? Color.red : Color.green, deltaTime);
#endif
            
            if (hit)
            {
                foreach (var entity in results)
                {
                    if (damageTickerType == DamageTickerType.DamageOverTime && !this.collisions.Add(entity)) continue;

                    if (!TryDamage(position, entity)) continue;

                    totalCollisions++;

                    if (totalCollisions == limitNumberCollisions)
                    {
                        EndLifeCycle();
                        return;
                    }
                }
            }
        }

        protected override void OnStop()
        {
            base.OnStop();

            projectile = null;
            
            OnComplete?.Invoke();
            OnComplete = null;
        }

        private bool TryDamage(Vector2 position, int entity)
        {
            switch (damageTickerType)
            {
                case DamageTickerType.Instant:
                    return Damage(position, entity);
                case DamageTickerType.DamageOverTime:
                    if (damageTickerElapsedTime < damageTickerInterval) return false;
                    if (Damage(position, entity))
                    {
                        damageTickerElapsedTime = 0;
                        return true;
                    }

                    return false;
                default:
                    Debug.LogError($"TryDamage: '{damageTickerType}' not defined ");
                    return false;
            }
        }

        private bool Damage(Vector2 position, int entity)
        {
            return onDamageEntity(entity, position);
        }
    }
}