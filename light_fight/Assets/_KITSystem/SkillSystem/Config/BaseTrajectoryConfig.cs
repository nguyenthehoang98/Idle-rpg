using System;

namespace _KITSystem.SkillSystem.Config
{
    [Serializable]
    public abstract class BaseTrajectoryConfig
    {
        public bool isRequireTargetToCast;
        
        public abstract TrajectoryType Type { get; }
        
        public abstract float Duration { get; }

        public enum TrajectoryType
        {
            Stationary,
            Bullet,
            Boomerang,
            Blend,
            Parabolic
        }
    }
}