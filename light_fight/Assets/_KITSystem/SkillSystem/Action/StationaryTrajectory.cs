using System;

namespace _KITSystem.SkillSystem.Action
{
    public partial class CastProjectileAction
    {
        public partial class RangerProjectile
        {
            [Serializable]
            public class StationaryTrajectory : BaseTrajectory
            {
                public TargetType targetType;
                public float radiusScanTarget;
                public bool isRequireTarget;
                public override TrajectoryType Type => TrajectoryType.Stationary;

                public enum TargetType
                {
                    Random,
                    Nearest,
                    Farthest,
                    HealthLowest,
                    HealthHighest,
                    DamageLowest,
                    DamageHighest,
                }
            }
        }
    }
}