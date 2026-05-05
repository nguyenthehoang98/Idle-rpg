using UnityEngine;

namespace _KITSystem.SkillSystem.Runtime
{
    internal abstract class BaseTrajectoryAction
    {
        protected Vector3 goal;
        protected Vector3 start;
        protected Vector3 direction;
        
        protected BaseTrajectoryAction(Vector3 start, Vector3 goal)
        {
            this.goal = goal;
            this.start = start;
            this.direction = (goal - start).normalized;
        }

        public Vector3 EvaluatePosition(float deltaTime)
        {
            return OnEvaluatePosition(deltaTime);
        }
        
        protected abstract Vector3 OnEvaluatePosition(float deltaTime);
    }
}