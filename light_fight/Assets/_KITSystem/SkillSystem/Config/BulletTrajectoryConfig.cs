using System;
using Sirenix.OdinInspector;

namespace _KITSystem.SkillSystem.Config
{
    [Serializable]
    public class BulletTrajectoryConfig : BaseTrajectoryConfig
    {
        [TitleGroup("Bullet")]
        public float initialSpeed;
        public float acceleration;
                
        public override TrajectoryType Type => TrajectoryType.Bullet;
        public override float Duration => 0;
    }
}