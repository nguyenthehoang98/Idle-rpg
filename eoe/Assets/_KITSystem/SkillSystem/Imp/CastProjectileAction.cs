using System;
using System.Collections.Generic;
using _Game.GamePlay;
using _Game.GamePlay.Model;
using _KITSystem.SkillSystem.Core;
using UnityEngine;

namespace _KITSystem.SkillSystem.Imp
{
    public struct DamageEntityInfo
    {
        public int entity;
        public bool isLastCollision;
        public Vector3 projectilePosition;

        public DamageEntityInfo(int entity, bool isLastCollision, Vector3 projectilePosition)
        {
            this.entity = entity;
            this.isLastCollision = isLastCollision;
            this.projectilePosition = projectilePosition;
        }
    }

    public class CastProjectileAction : BaseAction
    {
        private readonly List<BaseCollider> colliders;
        private readonly BaseTrajectory trajectory;
        private readonly float damageInterval;
        private readonly int limitNumberCollisions;
        private readonly float resetCollisionInterval;
        private readonly Func<DamageEntityInfo, bool> onDamageEntity;

        public event Action OnComplete;

        private Projectile projectile;
        private readonly HashSet<int> collisions = new HashSet<int>();
        private readonly List<int> results = new List<int>();

        private int totalCollisions;
        private float collisionResetElapsedTime;
        private float damageTickerElapsedTime;

        public CastProjectileAction(float lifeTime, List<BaseCollider> colliders, BaseTrajectory trajectory,
            Func<DamageEntityInfo, bool> onDamageEntity,
            Projectile projectile, float damageInterval,
            int limitNumberCollisions, float resetCollisionInterval) : base(lifeTime)
        {
            this.onDamageEntity = onDamageEntity;
            this.colliders = colliders;
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

            Vector2 position = trajectory.EvaluatePosition(deltaTime);

            Vector2 direction = trajectory.EvaluateDirection(deltaTime);

            projectile.SetPosition(position, direction, deltaTime);

            results.Clear();

            foreach (var collider in colliders)
            {
                collider.Tick(deltaTime);

                List<int> hits = collider.Collision(position, direction);

#if UNITY_EDITOR
                Color color = (hits != null && hits.Count > 0) ? Color.red : Color.green;

                collider.Gizmos(position, direction, color, deltaTime);
#endif

                if (hits != null && hits.Count > 0) results.AddRange(hits);
            }

            bool hit = results.Count > 0;

            if (hit)
            {
                foreach (var entity in results)
                {
                    bool dmgOverTime = damageInterval > 0;

                    if (dmgOverTime)
                    {
                        Debug.LogError("Chưa xử lý");
                    }
                    else
                    {
                        if (!collisions.Add(entity)) continue;

                        DamageEntityInfo info = new DamageEntityInfo(
                            entity, totalCollisions + 1 == limitNumberCollisions, position
                        );
                        
                        if (!TryDamage(info)) continue;

                        totalCollisions++;

                        if (totalCollisions == limitNumberCollisions)
                        {
                            Interrupt();

                            return;
                        }
                    }
                }
            }
        }

        protected override void OnStop()
        {
            base.OnStop();

            trajectory.Dispose();

            projectile = null;

            OnComplete?.Invoke();

            OnComplete = null;
        }

        private bool TryDamage(DamageEntityInfo info)
        {
            bool dmgOverTime = damageInterval > 0;

            if (dmgOverTime)
            {
                if (damageTickerElapsedTime < damageInterval) return false;

                if (onDamageEntity.Invoke(info))
                {
                    damageTickerElapsedTime = 0;
                    return true;
                }

                return false;
            }

            return onDamageEntity.Invoke(info);
        }
    }
}