using System;
using _KITSystem.SkillSystem.Config.Model;
using Sirenix.OdinInspector;

namespace _KITSystem.SkillSystem.Config.Action
{
    public partial class CastProjectileAction
    {
        public partial class RangerProjectile
        {
            [Serializable]
            public class BlendTrajectory : BaseTrajectory
            {
                [HideLabel, TitleGroup("Blend")]
                public BlendConstValue value = new BlendConstValue();
                public override TrajectoryType Type => TrajectoryType.Blend;
            }
        }
    }
}