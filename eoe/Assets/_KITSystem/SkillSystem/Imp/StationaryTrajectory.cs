using _KITSystem.SkillSystem.Core;
using UnityEngine;

namespace _KITSystem.SkillSystem.Imp
{
    public class StationaryTrajectory : BaseTrajectory
    {
        public StationaryTrajectory(Vector2 start, Vector2 goal) : base(start, goal)
        {
        }

        protected override Vector2 OnEvaluatePosition(float deltaTime)
        {
            return Start;
        }
    }
}