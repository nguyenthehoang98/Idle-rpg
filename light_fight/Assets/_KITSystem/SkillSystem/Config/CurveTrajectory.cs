using System;
using Sirenix.OdinInspector;

namespace _KITSystem.SkillSystem.Config
{
    [Serializable]
    public class BlendTrajectoryConfig : BaseTrajectoryConfig
    {
        public float duration = 1;
        [HideLabel, TitleGroup("Blend")]
        public BlendConstValue value = new BlendConstValue();
        public override TrajectoryType Type => TrajectoryType.Blend;
    }
}