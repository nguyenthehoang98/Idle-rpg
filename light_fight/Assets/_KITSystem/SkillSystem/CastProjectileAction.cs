using System;
using System.Collections.Generic;
using _KITSystem.Resource;
using _KITSystem.SkillSystem.Model;
using _KITSystem.Utils;
using UnityEngine;

namespace _KITSystem.SkillSystem
{
    internal class CastProjectileAction : BaseSkillAction
    {
        private readonly BaseShapeAction shapeAction;
        private readonly BaseTrajectoryAction trajectoryAction;
        private readonly DamageTickerType damageTickerType;
        private readonly float damageTickerInterval;
        private readonly int maxCollision;
        private readonly float resetCollisionInterval;
        private readonly Func<int, bool> onDamageEntity;

        private GameObject projectile;
        
        private HashSet<int> collisions;
        private int totalCollisions;
        private float collisionResetElapsedTime;
        
        private float damageTickerElapsedTime;
        
        private Vector2 previousPosition;
        private Vector2 direction;
        
        public CastProjectileAction(Spu spu, float lifeTime, BaseShapeAction shapeAction, BaseTrajectoryAction trajectoryAction,
            Func<int, bool> onDamageEntity,
            GameObject projectile, DamageTickerType damageTickerType, float damageTickerInterval,
            int maxCollision, float resetCollisionInterval) : base(spu, lifeTime)
        {
            this.onDamageEntity = onDamageEntity;
            this.shapeAction = shapeAction;
            this.trajectoryAction = trajectoryAction;
            this.projectile = projectile;
            this.damageTickerType = damageTickerType;
            this.maxCollision = maxCollision;
            this.resetCollisionInterval = resetCollisionInterval > 0 ? resetCollisionInterval : float.MaxValue;
            this.damageTickerInterval = damageTickerInterval;
            this.collisions = new HashSet<int>();
            this.collisionResetElapsedTime = this.damageTickerElapsedTime = 0;
            this.totalCollisions = 0;
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

            Vector2 position = this.trajectoryAction.EvaluatePosition(deltaTime);
            direction = MathUtils.NormalizeSafe(position - previousPosition);
            previousPosition = position;
            
            projectile.transform.position = position;
            
            shapeAction.Tick(deltaTime);
            
            List<int> results = shapeAction.Collision(position);

            if (results != null && results.Count > 0)
            {
                foreach (var entity in results)
                {
                    if (damageTickerType == DamageTickerType.DamageOverTime && !this.collisions.Add(entity)) continue;

                    if (!TryDamage(entity)) continue;

                    totalCollisions++;

                    if (totalCollisions == maxCollision)
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

            if (projectile != null)
            {
                KitPool.Destroy(projectile.gameObject);
                
                projectile = null;
            }
        }

        private bool TryDamage(int entity)
        {
            switch (damageTickerType)
            {
                case DamageTickerType.Instant:
                    return Damage(entity);
                case DamageTickerType.DamageOverTime:
                    if (damageTickerElapsedTime < damageTickerInterval) return false;
                    if (Damage(entity))
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

        private bool Damage(int entity)
        {
            return onDamageEntity(entity);
        }
    }
}
