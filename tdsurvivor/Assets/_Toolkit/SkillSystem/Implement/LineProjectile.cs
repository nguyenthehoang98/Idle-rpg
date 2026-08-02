using _Toolkit.SkillSystem.Core;
using UnityEngine;

namespace _Toolkit.SkillSystem.Implement
{
    public class LineProjectile : Projectile
    {
        [SerializeField] AnimationCurve animationCurve;

        float speed;
        float duration;
        float elapsedTime;

        protected override void OnStartup()
        {
            base.OnStartup();
            speed = Context.Speed;
            duration = Context.Duration;
            elapsedTime = 0;
        }

        protected override float TransitionDuration => duration;
        protected override Vector3 TransitionDeltaPosition { get; set; }
        protected override Vector3 TransitionPreviousPosition { get; set; }

        protected override void OnTick(float deltaTime)
        {
            TransitionPreviousPosition = transform.position;
            
            elapsedTime += deltaTime;
            float progress = TransitionDuration <0 ? 1 : Mathf.Clamp01(elapsedTime / TransitionDuration);
            float f = animationCurve.Evaluate(progress);

            TransitionDeltaPosition = Direction * (f * speed);
            if (progress >= 1f) Shutdown();
        }
    }
}