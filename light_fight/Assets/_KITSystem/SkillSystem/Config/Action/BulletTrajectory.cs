using System;
using Sirenix.OdinInspector;

namespace _KITSystem.SkillSystem.Config.Action
{
    public partial class CastProjectileAction
    {
        public partial class RangerProjectile
        {
            [Serializable]
            public class BulletTrajectory : BaseTrajectory
            {
                [TitleGroup("Bullet")]
                public float initialSpeed;
                public float acceleration;
                
                public override TrajectoryType Type => TrajectoryType.Bullet;
            }
        }
    }
}