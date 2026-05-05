using UnityEngine;

namespace _KITSystem.SkillSystem.Runtime
{
    internal abstract class BaseTrajectoryAction
    {
        private Vector3 goal;
        private Vector3 start;
        private Vector3 direction;

        protected BaseTrajectoryAction(Vector3 start, Vector3 goal)
        {
            this.goal = goal;
            this.start = start;
            this.direction = (goal - start).normalized;
        }

        public Vector3 EvaluatePosition(float deltaTime)
        {
            return OnEvaluatePosition(start, goal, direction, deltaTime);
        }
        
        protected abstract Vector3 OnEvaluatePosition(Vector3 start, Vector3 goal, Vector3 direction, float deltaTime);
    }
}