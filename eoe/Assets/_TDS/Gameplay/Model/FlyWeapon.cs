using _GameToolkit.Shared;
using LitMotion.Animation;
using UnityEngine;

namespace _TDS.Gameplay.Model
{
    [RequireComponent(typeof(Projectile))]
    public class FlyWeapon : BaseWeapon
    {
        [Header("Animation")]
        [SerializeField] private Animator animator;
        [SerializeField] private LitMotionAnimation startupAnimation;
        [SerializeField] private LitMotionAnimation phase01Animation;
        [SerializeField] private LitMotionAnimation phase02Animation;
        [SerializeField] private LitMotionAnimation completeAnimation;
        [SerializeField] private float revertRotateDuration = 0.15f;
        
        protected override bool UseWeapon => true;

        protected override void OnPlayAttack()
        {
            base.OnPlayAttack();

            animator.enabled = false;

            ExecuteAttack();
        }

        protected override void OnStopAttack()
        {
            base.OnStopAttack();

            animator.enabled = true;
        }

        public void Startup() => startupAnimation?.Play();

        public void Phase01() => phase01Animation?.Play();

        public void Phase02() => phase02Animation?.Play();

        public void Complete()
        {
            if (completeAnimation != null)
            {
                float duration = completeAnimation.Duration();

                completeAnimation.Play();

                Timing.CallDelayed(duration, StopAttack);
            }
            else
            {
                StopAttack();
            }
        }
    }
}