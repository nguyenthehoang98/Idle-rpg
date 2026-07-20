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
        public bool useProjectilePosition;

        public DamageEntityInfo(int entity, bool isLastCollision, Vector3 projectilePosition)
        {
            this.entity = entity;
            this.useProjectilePosition = true;
            this.isLastCollision = isLastCollision;
            this.projectilePosition = projectilePosition;
        }

        public DamageEntityInfo(int entity)
        {
            this.entity = entity;
            this.useProjectilePosition = false;
            this.isLastCollision = false;
            this.projectilePosition = Vector3.zero;
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
            this.resetCollisionInterval = resetCollisionInterval <= 0 ? float.MaxValue : resetCollisionInterval;
            this.damageInterval = damageInterval;
            this.collisionResetElapsedTime = this.damageTickerElapsedTime = 0;
            this.totalCollisions = 0;
        }

        protected override void OnStart()
        {
            base.OnStart();

            projectile.ImmediatelySetPosition(trajectory.EvaluatePosition(0));
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
            
            bool dmgOverTime = damageInterval > 0;

            if (hit)
            {
                foreach (var entity in results)
                {

                    if (dmgOverTime)
                    {
                        if(totalCollisions < limitNumberCollisions)
                        {
                            if (collisions.Add(entity)) totalCollisions++;
                        }
                    }
                    else
                    {
                        if (!collisions.Add(entity)) continue;

                        DamageEntityInfo info = new DamageEntityInfo(
                            entity, totalCollisions + 1 == limitNumberCollisions, position
                        );

                        if (!onDamageEntity.Invoke(info)) continue;

                        totalCollisions++;

                        if (totalCollisions == limitNumberCollisions)
                        {
                            Interrupt();

                            return;
                        }
                    }
                }
            }
            
            // DOT
            if (dmgOverTime)
            {
                damageTickerElapsedTime += deltaTime;

                if (damageTickerElapsedTime >= damageInterval)
                {
                    damageTickerElapsedTime = 0;

                    foreach (var entity in collisions)
                    {
                        DamageEntityInfo info = new DamageEntityInfo(entity);

                        onDamageEntity.Invoke(info);
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
    }
}