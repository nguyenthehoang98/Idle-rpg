using System;
using System.Collections.Generic;
using _Toolkit.Collider;
using _Toolkit.Shared;
using _Toolkit.SkillSystem.Core;

namespace _Toolkit.SkillSystem.Implement
{
    public class ProjectileSkillAction : BaseSkillAction
    {
        private readonly List<ICollisionDetector> detectors;
        private readonly float hitResetColliderInterval;
        private readonly float damageInterval;
        private readonly int maximumHit;

        public event Func<HitInfo, bool> OnDamaged;

        private readonly HashSet<Unique> currentColliders = new HashSet<Unique>();
        private float colliderElapsedTime;
        private float damageTickerElapsedTime;
        private int totalHit;

        public ProjectileSkillAction(float lifeTime, List<ICollisionDetector> detectors,
            float damageInterval, float hitResetColliderInterval, int maximumHit)
            : base(lifeTime)
        {
            this.detectors = detectors;
            this.damageInterval = damageInterval;
            this.hitResetColliderInterval = hitResetColliderInterval;
            this.maximumHit = maximumHit;
        }

        public override void Startup()
        {
            base.Startup();

            for (int i = 0; i < detectors.Count; i++)
            {
                detectors[i].Startup();
                detectors[i].OnOverlapped += Overlapped;
            }
        }

        private void Overlapped(Unique unique)
        {
            if (totalHit >= maximumHit)
            {
                Shutdown();
                return;
            }

            if (currentColliders.Add(unique))
            {
                HitInfo info = new HitInfo(unique, totalHit + 1 == maximumHit);

                if (OnDamaged != null && OnDamaged.Invoke(info))
                {
                    totalHit++;

                    if (totalHit == maximumHit)
                    {
                        Interrupt();
                    }
                }
            }
        }

        public override void Shutdown()
        {
            base.Shutdown();

            for (int i = 0; i < detectors.Count; i++)
            {
                detectors[i].OnOverlapped -= Overlapped;
                detectors[i].Shutdown();
            }
        }

        protected override void OnTick(float deltaTime)
        {
            // (1) todo: reset collider interval

            colliderElapsedTime += deltaTime;

            if (colliderElapsedTime >= hitResetColliderInterval && currentColliders.Count > 0)
            {
                currentColliders.Clear();

                colliderElapsedTime = 0;
            }

            // (2) todo: reset damage ticker, tính theo DOT

            damageTickerElapsedTime += deltaTime;

            if (damageInterval > 0 && damageTickerElapsedTime >= damageInterval)
            {
                foreach (var unique in currentColliders)
                {
                    HitInfo info = new HitInfo(unique, false);

                    if (OnDamaged != null) OnDamaged.Invoke(info);
                }

                damageTickerElapsedTime = 0;
            }
        }
    }
}