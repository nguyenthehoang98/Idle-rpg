using System;
using System.Collections.Generic;
using _Game.GamePlay;
using _Game.GamePlay.Model;
using _KITSystem.SkillSystem.Core;
using UnityEngine;

namespace _KITSystem.SkillSystem.Imp
{
    public class CastProjectileAction : BaseAction
    {
        private readonly BaseCollider collider;
        private readonly BaseTrajectory trajectory;
        private readonly float damageInterval;
        private readonly int limitNumberCollisions;
        private readonly float resetCollisionInterval;
        private readonly Func<int, Vector2, bool, bool> onDamageEntity;

        public event Action OnComplete;

        private Projectile projectile;
        private readonly HashSet<int> collisions = new HashSet<int>();

        private Vector2 previousPosition;
        private int totalCollisions;
        private float collisionResetElapsedTime;
        private float damageTickerElapsedTime;

        public CastProjectileAction(Spu spu, float lifeTime, BaseCollider collider, BaseTrajectory trajectory,
            Func<int, Vector2, bool, bool> onDamageEntity,
            Projectile projectile, float damageInterval,
            int limitNumberCollisions, float resetCollisionInterval) : base(spu, lifeTime)
        {
            this.onDamageEntity = onDamageEntity;
            this.collider = collider;
            this.trajectory = trajectory;
            this.projectile = projectile;
            this.limitNumberCollisions = limitNumberCollisions;
            this.resetCollisionInterval = resetCollisionInterval > 0 ? resetCollisionInterval : float.MaxValue;
            this.damageInterval = damageInterval;
            this.collisionResetElapsedTime = this.damageTickerElapsedTime = 0;
            this.totalCollisions = 0;
        }

        protected override void OnUpdate(float deltaTime)
        {
            collisionResetElapsedTime += deltaTime;

            if (collisionResetElapsedTime >= resetCollisionInterval && collisions.Count > 0)
            {
                collisions.Clear();
                collisionResetElapsedTime = 0;
            }

            damageTickerElapsedTime += deltaTime;

            Vector2 currentPosition = trajectory.EvaluatePosition(deltaTime);
            
            projectile.SetPosition(currentPosition, deltaTime);

            collider.Tick(deltaTime);

            List<int> results = collider.Collision(previousPosition, currentPosition);

            bool hit = results != null && results.Count > 0;

#if UNITY_EDITOR
            Color color = hit ? Color.red : Color.green;
            Debug.DrawLine(previousPosition, currentPosition, color, deltaTime);
            collider.Gizmos(previousPosition, currentPosition, color, deltaTime);
#endif

            previousPosition = currentPosition;
            
            if (hit)
            {
                foreach (var entity in results)
                {
                    bool dmgOverTime = damageInterval > 0;
                    if (dmgOverTime && !collisions.Add(entity)) continue;

                    if (!TryDamage(currentPosition, entity, totalCollisions + 1 == limitNumberCollisions)) continue;

                    totalCollisions++;
                    
                    if (totalCollisions == limitNumberCollisions)
                    {
                        Interrupt();
                        
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

        private bool TryDamage(Vector2 position, int entity, bool lastCollision)
        {
            bool dmgOverTime = damageInterval > 0;

            if (dmgOverTime)
            {
                if (damageTickerElapsedTime < damageInterval) return false;
                if (Damage(position, entity, lastCollision))
                {
                    damageTickerElapsedTime = 0;
                    return true;
                }

                return false;
            }
            else
            {
                return Damage(position, entity, lastCollision);
            }
        }

        private bool Damage(Vector2 position, int entity, bool lastCollision)
        {
            return onDamageEntity(entity, position, lastCollision);
        }
    }
}