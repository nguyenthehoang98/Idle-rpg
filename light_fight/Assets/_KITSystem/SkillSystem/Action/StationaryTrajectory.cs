using System;
using Sirenix.OdinInspector;

namespace _KITSystem.SkillSystem.Action
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