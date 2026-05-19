using _KITSystem.SkillSystem.Config;
using Unity.Mathematics;

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
        private float2 endPosition;

        public BoomerangTrajectoryAction(BlendConstValue forward, float forwardDuration,
            BlendConstValue backward, float backwardDuration,
            float2 start, float2 goal) : base(start, goal)
        {
            this.forward = forward;
            this.forwardDuration = forwardDuration;
            this.backward = backward;
            this.backwardDuration = backwardDuration;
            isForwardPhase = true;
        }

        protected override float2 OnEvaluatePosition(float deltaTime)
        {
            elapsedTime += deltaTime;
            float f;
            float d;
            if (isForwardPhase)
            {
                f = math.clamp(elapsedTime / forwardDuration, 0, 1);
                d = forward.Evaluate(f);
                float2 s = d * direction;
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
                f = math.clamp(elapsedTime / backwardDuration, 0, 1);
                d = backward.Evaluate(f);
                return endPosition + d * -direction;
            }
        }
    }
}