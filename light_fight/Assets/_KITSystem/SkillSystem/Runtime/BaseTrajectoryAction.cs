using _KITSystem.Utils;
using Unity.Mathematics;

namespace _KITSystem.SkillSystem.Runtime
{
    internal abstract class BaseTrajectoryAction
    {
        protected float2 goal;
        protected float2 start;
        protected float2 direction;
        
        protected BaseTrajectoryAction(float2 start, float2 goal)
        {
            this.goal = goal;
            this.start = start;
            direction = MathUtils.NormalizeSafe(goal - start);
        }

        public float2 EvaluatePosition(float deltaTime)
        {
            return OnEvaluatePosition(deltaTime);
        }
        
        protected abstract float2 OnEvaluatePosition(float deltaTime);
    }
}