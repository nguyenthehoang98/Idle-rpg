using _KITSystem.SkillSystem.Config;
using UnityEngine;

namespace _KITSystem.SkillSystem.Runtime
{
    internal class BlendTrajectoryAction : BaseTrajectoryAction
    {
        private BlendConstValue value;

        private float duration;
        private float elapsedTime;
        
        public BlendTrajectoryAction(BlendConstValue value, float duration, Vector3 start, Vector3 goal) : base(start, goal)
        {
            this.value = value;
            this.duration = duration;
        }

        protected override Vector3 OnEvaluatePosition(float deltaTime)
        {
            elapsedTime += deltaTime;
            float f = Mathf.Clamp01(elapsedTime / duration);
            float d = value.Evaluate(f);
            return start + d * direction;
        }
    }
}