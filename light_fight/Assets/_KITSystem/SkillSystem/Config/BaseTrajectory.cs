using System;

namespace _KITSystem.SkillSystem.Config
{
    [Serializable]
    public abstract class BaseTrajectory
    {
        public bool isRequireTargetToCast;
        public abstract TrajectoryType Type { get; }

        public enum TrajectoryType
        {
            Stationary,
            Bullet,
            Cannon,
            Boomerang,
            Blend,
            Parabolic
        }
    }
}