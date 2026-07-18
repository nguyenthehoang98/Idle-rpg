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
        [SerializeField] private LitMotionAnimation outboundAnimation;
        [SerializeField] private LitMotionAnimation hangAnimation;
        [SerializeField] private LitMotionAnimation returnAnimation;
        [SerializeField] private LitMotionAnimation completeAnimation;
        [SerializeField] private float revertRotateDuration = 0.15f;
        
        protected override bool IsFlyWeapon => true;

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

        public void OutboundFly() => outboundAnimation?.Play();

        public void HangFly() => hangAnimation?.Play();

        public void ReturnFly() => returnAnimation?.Play();

        public void CompleteFly()
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