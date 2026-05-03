using System;

namespace _KITSystem.SkillSystem.Config.Action
{
    public partial class CastProjectileAction
    {
        public partial class RangerProjectile
        {
            [Serializable]
            public class StationaryTrajectory : BaseTrajectory
            {
                public override TrajectoryType Type => TrajectoryType.Stationary;
            }
        }
    }
}