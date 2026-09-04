using System;
using System.Collections.Generic;
using _GameToolkit.Colliders;
using _GameToolkit.Share;
using UnityEngine;

namespace _GameToolkit.Skills
{
    public class ProjectileSkillAction : SkillAction
    {
        private readonly CollisionDetector[] detectors;
        private readonly float hitInterval;
        private readonly float damageInterval;
        private readonly int hitCount;
        private readonly float collisionDelayInit;
        private readonly float collisionDuration;
        
        public event Func<HitInfo, bool> OnDamaged;
        
        private readonly HashSet<Unique> currentColliders = new HashSet<Unique>();
        private float colliderElapsedTime;
        private float damageTickerElapsedTime;
        private float actionElapsedTime;
        private int totalHit;
        private bool detectorsStarted;
        
        public ProjectileSkillAction(float lifeTime, CollisionDetector[] detectors,
            float damageInterval, float hitInterval, int hitCount,
            float collisionDelayInit = 0, float collisionDuration = 0) : base(lifeTime)
        {
            this.detectors = detectors;
            this.damageInterval = damageInterval;
            this.hitInterval = hitInterval;
            this.hitCount = hitCount;
            this.collisionDelayInit = Mathf.Max(0, collisionDelayInit);
            this.collisionDuration = Mathf.Max(0, collisionDuration);
        }

        public override void Startup()
        {
            base.Startup();
            for (int i = 0; i < detectors.Length; i++)
            {
                detectors[i].OnOverlapped += Overlapped;
            }

            // collisionDelayInit > 0: chờ tới thời điểm đó mới bật detector
            if (collisionDelayInit <= 0) StartDetectors();
        }

        private void StartDetectors()
        {
            if (detectorsStarted) return;
            detectorsStarted = true;
            for (int i = 0; i < detectors.Length; i++) detectors[i].Startup();
        }

        private void StopDetectors()
        {
            if (!detectorsStarted) return;
            detectorsStarted = false;
            for (int i = 0; i < detectors.Length; i++) detectors[i].Shutdown();
        }
        
        public override void Shutdown()
        {
            base.Shutdown();
            StopDetectors();
            for (int i = 0; i < detectors.Length; i++)
            {
                detectors[i].OnOverlapped -= Overlapped;
            }
        }
        
        protected override void OnTick(float deltaTime)
        {
            actionElapsedTime += deltaTime;

            // cửa sổ va chạm: bật sau collisionDelayInit, tắt sau collisionDuration (nếu > 0)
            if (!detectorsStarted && actionElapsedTime >= collisionDelayInit)
            {
                StartDetectors();
            }
            else if (detectorsStarted && collisionDuration > 0 &&
                     actionElapsedTime >= collisionDelayInit + collisionDuration)
            {
                StopDetectors();
            }

            // (1) reset collider interval
            colliderElapsedTime += deltaTime;

            if (colliderElapsedTime >= hitInterval && currentColliders.Count > 0)
            {
                currentColliders.Clear();
                colliderElapsedTime = 0;
            }

            // (2) reset damage ticker, tính theo DOT
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
        
        private void Overlapped(Unique unique)
        {
            if (totalHit >= hitCount)
            {
                Debug.Log($"[PSA] blocked overlap: totalHit={totalHit} >= hitCount={hitCount}");
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
            else
            {
                Debug.Log($"[PSA] DUP overlap unique={unique.name} ignored (already counted this interval)");
            }
        }
    }
}
