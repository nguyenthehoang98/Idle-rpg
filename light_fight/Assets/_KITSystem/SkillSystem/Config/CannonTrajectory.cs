using System;
using Sirenix.OdinInspector;

namespace _KITSystem.SkillSystem.Config
{
    public partial class CastProjectileAction
    {
        public partial class RangerProjectile
        {
            [Serializable]
            public class CannonTrajectory : BaseTrajectory
            {         
                [TitleGroup("Cannon")]
                public float initialSpeed;
                public float gravity;
                public float delay;
                public override TrajectoryType Type => TrajectoryType.Cannon;
            }
        }
    }
}