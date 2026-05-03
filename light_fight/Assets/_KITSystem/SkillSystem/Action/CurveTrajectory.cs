using System;
using _KITSystem.SkillSystem.Model;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _KITSystem.SkillSystem.Action
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