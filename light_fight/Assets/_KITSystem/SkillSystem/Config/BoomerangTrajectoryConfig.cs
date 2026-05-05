using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _KITSystem.SkillSystem.Config
{
    [Serializable]
    public class BoomerangTrajectoryConfig : BaseTrajectoryConfig
    {
        [TitleGroup("Boomerang: Cast phase")]
        public float castDuration;
        [HideLabel] public BlendConstValue castPhase = new BlendConstValue(BlendConstValue.BlendConstType.Blend);
              
        [TitleGroup("Boomerang: Return phase")]
        public float returnDuration;
        [HideLabel] public BlendConstValue returnPhase = new BlendConstValue(BlendConstValue.BlendConstType.Blend);

        public override TrajectoryType Type => TrajectoryType.Boomerang;
        public override float Duration => castDuration + returnDuration;
    }
}