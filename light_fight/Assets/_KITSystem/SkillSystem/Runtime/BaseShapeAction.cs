using System.Collections.Generic;
using _KITSystem.SkillSystem.Config;
using Unity.Mathematics;
using UnityEngine;

namespace _KITSystem.SkillSystem.Runtime
{
    internal abstract class BaseShapeAction
    {
        private float triggerTimeInSeconds;
        private float elapsed;
        private float2 offsetRelativePosition;

        public bool CanTrigger { get; private set; }

        protected BaseShapeAction(BaseShapeConfig shapeConfig)
        {
            triggerTimeInSeconds = shapeConfig.triggerTimeInSeconds;
            offsetRelativePosition = shapeConfig.offsetRelativePosition;
        }

        protected float2 GetPosition(float2 position)
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

        public bool Hit(float2 position, out List<int> hitsId)
        {
            if (CanTrigger)
            {
                return OnHit(position, out hitsId);
            }

            hitsId = null;
            return false;
        }

        protected abstract bool OnHit(float2 position, out List<int> hitsId);

        public virtual void Gizmos(Vector3 position, Color color, float duration)
        {
        }
    }
}