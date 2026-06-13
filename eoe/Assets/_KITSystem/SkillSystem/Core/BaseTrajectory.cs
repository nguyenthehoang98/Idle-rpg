using _KITSystem.Utils;
using UnityEngine;

namespace _KITSystem.SkillSystem.Core
{
    public abstract class BaseTrajectory
    {
        protected Vector2 Goal;
        protected Vector2 Start;
        protected Vector2 Direction;
        
        protected BaseTrajectory(Vector2 start, Vector2 goal)
        {
            Goal = goal;
            Start = start;
            Direction = MathUtils.NormalizeSafe(goal - start);
        }

        public Vector2 EvaluatePosition(float deltaTime)
        {
            return OnEvaluatePosition(deltaTime);
        }
        
        protected abstract Vector2 OnEvaluatePosition(float deltaTime);
    }
}