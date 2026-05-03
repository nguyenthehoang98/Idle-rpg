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
            public class BoomerangTrajectory : BaseTrajectory
            {
                [HideLabel, TitleGroup("Boomerang: Cast phase")]
                public BlendConstValue castPhase = new BlendConstValue();
              
                [HideLabel, TitleGroup("Boomerang: Return phase")]
                public BlendConstValue returnPhase = new BlendConstValue();
                public override TrajectoryType Type => TrajectoryType.Boomerang;
            }
        }
    }
}