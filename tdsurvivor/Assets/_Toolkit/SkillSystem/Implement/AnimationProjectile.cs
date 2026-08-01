using _Toolkit.SkillSystem.Core;
using UnityEngine;

namespace _Toolkit.SkillSystem.Implement
{
    public class AnimationProjectile : Projectile
    {
        [SerializeField] private Animator animator;
        [SerializeField] private string animationClipName;

        protected override void OnStartup()
        {
            base.OnStartup();
            animator.Play(animationClipName);
        }

        protected override void OnTick(float deltaTime)
        {
        }

        /*
         * Gọi từ animation
         */
        public void AnimationCompleted() => Shutdown();

        protected override float TransitionDuration => 0;
        protected override Vector3 TransitionDeltaPosition { get; set; }
        protected override Vector3 TransitionPreviousPosition { get; set; }
    }
}