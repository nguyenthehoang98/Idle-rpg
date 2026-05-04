using System;

namespace _KITSystem.SkillSystem.Config
{
    [Serializable]
    public class StationaryTrajectory : BaseTrajectory
    {
        public override TrajectoryType Type => TrajectoryType.Stationary;
    }
}