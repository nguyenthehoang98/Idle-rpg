using _KITSystem.SkillSystem.Config;
using UnityEngine;

namespace _KITSystem.SkillSystem.Runtime
{
    internal class BoomerangTrajectoryAction : BaseTrajectoryAction
    {
        private BlendConstValue forward;
        private float forwardDuration;
        private BlendConstValue backward;
        private float backwardDuration;
        private float elapsedTime;
        private bool isForwardPhase;
        private Vector3 endPosition;

        public BoomerangTrajectoryAction(BlendConstValue forward, float forwardDuration,
            BlendConstValue backward, float backwardDuration,
            Vector3 start, Vector3 goal) : base(start, goal)
        {
            this.forward = forward;
            this.forwardDuration = forwardDuration;
            this.backward = backward;
            this.backwardDuration = backwardDuration;
            isForwardPhase = true;
        }

        protected override Vector3 OnEvaluatePosition(float deltaTime)
        {
            elapsedTime += deltaTime;
            float f;
            float d;
            if (isForwardPhase)
            {
                f = Mathf.Clamp01(elapsedTime / forwardDuration);
                d = forward.Evaluate(f);
                Vector3 s = d * direction;
                if (elapsedTime >= forwardDuration)
                {
                    endPosition = s;
                    elapsedTime = 0;
                    isForwardPhase = false;
                }

                return s;
            }
            else
            {
                f = Mathf.Clamp01(elapsedTime / backwardDuration);
                d = backward.Evaluate(f);
                return endPosition + d * -direction;
            }
        }
    }
}