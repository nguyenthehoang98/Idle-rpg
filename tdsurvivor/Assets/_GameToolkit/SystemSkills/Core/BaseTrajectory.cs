using System;
using _KITSystem.Utils;
using UnityEngine;

namespace _KITSystem.SkillSystem.Core
{
    public abstract class BaseTrajectory : IDisposable
    {
        protected Vector2 Goal;
        protected Vector2 Start;
        protected Vector2 Direction;
        
        protected BaseTrajectory(Vector2 start, Vector2 goal)
        {
            Start = start;
            Goal = goal;
            Direction = (goal - start).normalized;
        }

        public abstract Vector2 EvaluatePosition(float deltaTime);

        public abstract Vector2 EvaluateDirection(float deltaTime);

        public virtual void Dispose()
        {
            // TODO release managed resources here
        }
    }
}