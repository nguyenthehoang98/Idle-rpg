using System;
using Sirenix.OdinInspector;

namespace _KITSystem.SkillSystem.Config
{
    [Serializable]
    public class ParabolicTrajectoryConfig : BaseTrajectoryConfig
    {            
        public float duration;
        [HideLabel, TitleGroup("Parabolic: Height")]
        public BlendConstValue height = new BlendConstValue(BlendConstValue.BlendConstType.Blend);
        [HideLabel, TitleGroup("Parabolic: Distance")]
        public BlendConstValue distance = new BlendConstValue(BlendConstValue.BlendConstType.Blend);
      
        public override TrajectoryType Type => TrajectoryType.Parabolic;
        public override float Duration => duration;
    }
}