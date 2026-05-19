using Unity.Mathematics;

namespace _KITSystem.SkillSystem.Runtime
{
    internal class StationaryTrajectoryAction : BaseTrajectoryAction
    {
        public StationaryTrajectoryAction(float2 start, float2 goal) : base(start, goal)
        {
        }

        protected override float2 OnEvaluatePosition(float deltaTime)
        {
            return goal;
        }
    }
}