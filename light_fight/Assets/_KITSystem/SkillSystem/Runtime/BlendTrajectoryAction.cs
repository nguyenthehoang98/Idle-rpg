using _KITSystem.SkillSystem.Config;
using Unity.Mathematics;

namespace _KITSystem.SkillSystem.Runtime
{
    internal class BlendTrajectoryAction : BaseTrajectoryAction
    {
        private BlendConstValue value;

        private float duration;
        private float elapsedTime;
        
        public BlendTrajectoryAction(BlendConstValue value, float duration, float2 start, float2 goal) : base(start, goal)
        {
            this.value = value;
            this.duration = duration;
        }

        protected override float2 OnEvaluatePosition(float deltaTime)
        {
            elapsedTime += deltaTime;
            float f = math.clamp(elapsedTime / duration, 0, 1);
            float d = value.Evaluate(f);
            return start + d * direction;
        }
    }
}