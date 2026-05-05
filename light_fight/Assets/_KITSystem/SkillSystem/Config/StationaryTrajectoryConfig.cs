using System;

namespace _KITSystem.SkillSystem.Config
{
    [Serializable]
    public class StationaryTrajectoryConfig : BaseTrajectoryConfig
    {
        public override TrajectoryType Type => TrajectoryType.Stationary;
    }
}