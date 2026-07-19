using _Game.GamePlay.Data;
using _KITSystem.Utils;
using LitMotion.Animation;
using UnityEngine;

namespace _Game.GamePlay.Model
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

        public override void OnStopAttack()
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

                this.WaitInvoke(duration, OnStopAttack);
            }
            else
            {
                OnStopAttack();
            }
        }
    }
}