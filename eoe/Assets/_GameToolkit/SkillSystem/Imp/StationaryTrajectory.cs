using _KITSystem.SkillSystem.Core;
using UnityEngine;

namespace _KITSystem.SkillSystem.Imp
{
    public class StationaryTrajectory : BaseTrajectory
    {
        public StationaryTrajectory(Vector2 start, Vector2 goal) : base(start, goal)
        {
        }

        public override Vector2 EvaluatePosition(float deltaTime)
        {
            return Goal;
        }

        public override Vector2 EvaluateDirection(float deltaTime)
        {
            return Direction;
        }
    }
}