using System;
using System.Collections.Generic;
using _Toolkit.Collider;
using _Toolkit.Shared;
using _Toolkit.SkillSystem.Core;

namespace _Toolkit.SkillSystem.Implement
{
    public class ProjectileSkillAction : BaseSkillAction
    {
        private readonly ICollisionDetector[] detectors;
        private readonly float hitInterval;
        private readonly float damageInterval;
        private readonly int hitCount;

        public event Func<HitInfo, bool> OnDamaged;

        private readonly HashSet<Unique> currentColliders = new HashSet<Unique>();
        private float colliderElapsedTime;
        private float damageTickerElapsedTime;
        private int totalHit;

        public ProjectileSkillAction(float lifeTime, ICollisionDetector[] detectors,
            float damageInterval, float hitInterval, int hitCount)
            : base(lifeTime)
        {
            this.detectors = detectors;
            this.damageInterval = damageInterval;
            this.hitInterval = hitInterval;
            this.hitCount = hitCount;
        }

        public override void Startup()
        {
            base.Startup();

            for (int i = 0; i < detectors.Length; i++)
            {
                detectors[i].Startup();
                detectors[i].OnOverlapped += Overlapped;
            }
        }

        private void Overlapped(Unique unique)
        {
            if (totalHit >= hitCount)
            {
                Shutdown();
                return;
            }

            if (currentColliders.Add(unique))
            {
                HitInfo info = new HitInfo(unique, totalHit + 1 == hitCount);

                if (OnDamaged != null && OnDamaged.Invoke(info))
                {
                    totalHit++;

                    if (totalHit == hitCount)
                    {
                        Interrupt();
                    }
                }
            }
        }

        public override void Shutdown()
        {
            base.Shutdown();

            for (int i = 0; i < detectors.Length; i++)
            {
                detectors[i].OnOverlapped -= Overlapped;
                detectors[i].Shutdown();
            }
        }

        protected override void OnTick(float deltaTime)
        {
            // (1) todo: reset collider interval

            colliderElapsedTime += deltaTime;

            if (colliderElapsedTime >= hitInterval && currentColliders.Count > 0)
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