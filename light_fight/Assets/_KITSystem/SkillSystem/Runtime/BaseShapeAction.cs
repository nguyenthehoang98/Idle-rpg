using System.Collections.Generic;
using _KITSystem.SkillSystem.Config;
using UnityEngine;

namespace _KITSystem.SkillSystem.Runtime
{
    internal abstract class BaseShapeAction
    {
        private float triggerTimeInSeconds;
        private float elapsed;
        private Vector3 offsetRelativePosition;

        public bool CanTrigger { get; private set; }

        protected BaseShapeAction(BaseShapeConfig shapeConfig)
        {
            triggerTimeInSeconds = shapeConfig.triggerTimeInSeconds;
            offsetRelativePosition = shapeConfig.offsetRelativePosition;
        }

        protected Vector3 GetPosition(Vector3 position)
        {
            return position + offsetRelativePosition;
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

        public bool Hit(Vector3 position, out List<int> hitsId)
        {
            if (CanTrigger)
            {
                return OnHit(position, out hitsId);
            }

            hitsId = null;
            return false;
        }

        protected abstract bool OnHit(Vector3 position, out List<int> hitsId);

        public virtual void Gizmos(Vector3 position, Color color, float duration)
        {
        }
    }
}