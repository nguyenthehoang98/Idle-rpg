using System;
using System.Collections.Generic;
using UnityEngine;

namespace _GameToolkit.SkillSystem.Core
{
    /// <summary>
    /// Concrete action: điều khiển 1 projectile bay theo trajectory, kiểm tra va chạm
    /// qua collider list, gọi callback khi trúng entity.
    ///
    /// Port từ Recovery 2 (_KITSystem.SkillSystem.Imp.CastProjectileAction) nhưng
    /// tách khỏi MonoBehaviour/game entity:
    /// - Visual: IProjectileView (game implement)
    /// - Hit data: HitInfo (generic, không phải DamageEntityInfo)
    /// - Damage logic: Func&lt;HitInfo, bool&gt; callback (game xử lý)
    /// </summary>
    public class CastProjectileAction : BaseAction
    {
        private readonly List<BaseCollider> colliders;
        private readonly BaseTrajectory trajectory;
        private readonly float damageInterval;
        private readonly int maxHitCount;
        private readonly float targetHitCooldown;
        private readonly Func<HitInfo, bool> onDamageEntity;

        public event Action OnComplete;

        private readonly IProjectileView projectile;
        private readonly HashSet<int> collisions = new HashSet<int>();
        private readonly List<int> results = new List<int>();

        private int totalCollisions;
        private float collisionResetElapsedTime;
        private float damageTickerElapsedTime;

        public CastProjectileAction(float lifeTime, List<BaseCollider> colliders, BaseTrajectory trajectory,
            Func<HitInfo, bool> onDamageEntity,
            IProjectileView projectile, float damageInterval,
            int maxHitCount, float targetHitCooldown) : base(lifeTime)
        {
            this.onDamageEntity = onDamageEntity;
            this.colliders = colliders;
            this.trajectory = trajectory;
            this.projectile = projectile;
            this.maxHitCount = maxHitCount;
            this.targetHitCooldown = targetHitCooldown <= 0 ? float.MaxValue : targetHitCooldown;
            this.damageInterval = damageInterval;
            this.collisionResetElapsedTime = this.damageTickerElapsedTime = 0;
            this.totalCollisions = 0;
        }

        protected override void OnUpdate(float deltaTime)
        {
            collisionResetElapsedTime += deltaTime;

            if (collisionResetElapsedTime >= targetHitCooldown && collisions.Count > 0)
            {
                collisions.Clear();

                collisionResetElapsedTime = 0;
            }

            damageTickerElapsedTime += deltaTime;

            Vector2 position = trajectory.EvaluatePosition(deltaTime);

            Vector2 direction = trajectory.EvaluateDirection(deltaTime);

            projectile?.SetPosition(position, direction, deltaTime);

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

            bool unlimitedHits = maxHitCount <= 0;

            if (hit)
            {
                foreach (var entity in results)
                {
                    if (dmgOverTime)
                    {
                        // DOT: chỉ đăng ký entity vào danh sách, sát thương tính theo tick
                        if (unlimitedHits || totalCollisions < maxHitCount)
                        {
                            if (collisions.Add(entity)) totalCollisions++;
                        }
                    }
                    else
                    {
                        if (!collisions.Add(entity)) continue;

                        bool isLast = !unlimitedHits && totalCollisions + 1 >= maxHitCount;

                        HitInfo info = new HitInfo(entity, position, isLast);

                        if (!onDamageEntity.Invoke(info)) continue;

                        totalCollisions++;

                        if (isLast)
                        {
                            Interrupt();

                            return;
                        }
                    }
                }
            }

            // DOT: ticker đã được cộng ở đầu OnUpdate — không cộng lại lần nữa
            if (dmgOverTime && damageTickerElapsedTime >= damageInterval)
            {
                damageTickerElapsedTime = 0;

                foreach (var entity in collisions)
                {
                    HitInfo info = new HitInfo(entity);

                    onDamageEntity.Invoke(info);
                }
            }
        }

        protected override void OnStop()
        {
            base.OnStop();

            trajectory.Dispose();

            projectile?.Destroy();

            OnComplete?.Invoke();

            OnComplete = null;
        }
    }
}
