using _Toolkit.SkillSystem.Core;
using UnityEngine;

namespace _Toolkit.SkillSystem.Implement
{
    public class StationaryProjectile : Projectile
    {
        protected override void OnStartup()
        {
            base.OnStartup();

            transform.position = Destination;
        }

        protected override void OnTick(float deltaTime)
        {
        }

        protected override float TransitionDuration => 0;
        protected override Vector3 TransitionDeltaPosition { get; set; }
        protected override Vector3 TransitionPreviousPosition { get; set; }
    }
}