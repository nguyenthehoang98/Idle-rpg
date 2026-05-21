using System;
using System.Collections.Generic;
using _KITSystem.SkillSystem.Config;
using Unity.Mathematics;
using UnityEngine;

namespace _KITSystem.SkillSystem.Runtime
{
    internal abstract class BaseShapeAction
    {
        protected IQuery query;
        private float triggerTimeInSeconds;
        private float elapsed;
        private float2 offsetRelativePosition;

        public bool CanTrigger { get; private set; }

        protected BaseShapeAction(BaseShapeConfig shapeConfig, IQuery query)
        {
            this.query = query;
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

        public List<int> Hit(float2 position)
        {
            if (CanTrigger)
                OnHit(position);
            return null;
        }

        protected abstract List<int> OnHit(float2 position);

        public virtual void Gizmos(Vector3 position, Color color, float duration)
        {
        }
    }
}