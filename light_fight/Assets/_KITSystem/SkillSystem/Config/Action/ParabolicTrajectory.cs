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
            public class ParabolicTrajectory : BaseTrajectory
            {               
                [HideLabel, TitleGroup("Parabolic: Height")]
                public BlendConstValue height = new BlendConstValue();
                [HideLabel, TitleGroup("Parabolic: Distance")]
                public BlendConstValue distance = new BlendConstValue();
                [HideLabel, TitleGroup("Parabolic: Speed")]
                public BlendConstValue speed = new BlendConstValue();
                public override TrajectoryType Type => TrajectoryType.Parabolic;
            }
        }
    }
}