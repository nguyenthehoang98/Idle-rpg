using System.Collections.Generic;
using _KITSystem.SkillSystem.Config;
using UnityEngine;

namespace _KITSystem.SkillSystem.Runtime
{
    internal abstract class BaseShapeAction
    {
        private readonly BaseShape square;
        private float triggerTimeInSeconds;
        private float elapsed;
        private bool canTrigger;
            
        protected BaseShapeAction(BaseShape square)
        {
            this.square = square;
            triggerTimeInSeconds = square.triggerTimeInSeconds;
        }

        public void Tick(float deltaTime)
        {
            if (canTrigger) return;

            elapsed += deltaTime;

            if (elapsed >= triggerTimeInSeconds)
            {
                canTrigger = true;
            }
        }

        public bool Hit(Vector3 position, out List<OwnGameObject> targets)
        {
            if (canTrigger)
            {
                return OnHit(position, out targets);
            }

            targets = null;
            return false;
        }

        protected abstract bool OnHit(Vector3 position, out List<OwnGameObject> targets);
    }
}