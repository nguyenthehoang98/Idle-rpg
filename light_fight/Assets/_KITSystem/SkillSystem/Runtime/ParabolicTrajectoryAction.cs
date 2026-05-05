using _KITSystem.SkillSystem.Config;
using UnityEngine;

namespace _KITSystem.SkillSystem.Runtime
{
    internal class ParabolicTrajectoryAction : BaseTrajectoryAction
    {
        private BlendConstValue height;
        private BlendConstValue distance;

        private float duration;
        private float elapsedTime;

        public ParabolicTrajectoryAction(BlendConstValue height, BlendConstValue distance,
            float duration, Vector3 start, Vector3 goal) : base(start, goal)
        {
            this.duration = duration;
            this.height = height;
            this.distance = distance;
            this.duration = duration;
        }

        protected override Vector3 OnEvaluatePosition(float deltaTime)
        {
            elapsedTime += deltaTime;
            float f = Mathf.Clamp01(elapsedTime / duration);
            float h = height.Evaluate(f);
            float d = distance.Evaluate(f);
            Vector3 cross = Vector3.Cross(direction, Vector3.back);
            return start + direction * d + cross * h;
        }
    }
}