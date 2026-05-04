using UnityEngine;

namespace _KITSystem.SkillSystem.Runtime
{
    internal abstract partial class CastProjectileAction
    {
        internal abstract class BaseHitBoxAction
        {
            private float triggerTimeInSeconds;
            private float elapsed;
            
            protected BaseHitBoxAction(Config.Action.CastProjectileAction.BaseHitBox hitBox)
            {
                triggerTimeInSeconds = hitBox.triggerTimeInSeconds;
            }

            public void Tick(float deltaTime)
            {
                if (CanTrigger) return;

                elapsed += deltaTime;

                if (elapsed >= triggerTimeInSeconds)
                {
                    CanTrigger = true;
                }
            }

            public abstract void Scan(Vector3 position);

            protected bool CanTrigger { get; private set; }
        }
        
        
    }
}