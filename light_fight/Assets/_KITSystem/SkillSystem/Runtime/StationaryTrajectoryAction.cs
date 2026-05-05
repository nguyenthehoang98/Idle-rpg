using UnityEngine;

namespace _KITSystem.SkillSystem.Runtime
{
    internal class StationaryTrajectoryAction : BaseTrajectoryAction
    {
        public StationaryTrajectoryAction(Vector3 start, Vector3 goal) : base(start, goal)
        {
        }

        protected override Vector3 OnEvaluatePosition(float deltaTime)
        {
            return goal;
        }
    }
}