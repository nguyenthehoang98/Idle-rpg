using System;
using Sirenix.OdinInspector;

namespace _KITSystem.SkillSystem.Config
{
    [Serializable]
    public class BoomerangTrajectoryConfig : BaseTrajectoryConfig
    {
        [HideLabel, TitleGroup("Boomerang: Cast phase")]
        public BlendConstValue castPhase = new BlendConstValue();
              
        [HideLabel, TitleGroup("Boomerang: Return phase")]
        public BlendConstValue returnPhase = new BlendConstValue();
        public override TrajectoryType Type => TrajectoryType.Boomerang;
    }
}