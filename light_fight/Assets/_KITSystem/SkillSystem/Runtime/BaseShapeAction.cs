using System;
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

        public void Hit(float2 position, Action<List<int>> callback)
        {
            if (CanTrigger) OnHit(position, callback);
        }

        protected abstract void OnHit(float2 position, Action<List<int>> callback);

        public virtual void Gizmos(Vector3 position, Color color, float duration)
        {
        }
    }
}