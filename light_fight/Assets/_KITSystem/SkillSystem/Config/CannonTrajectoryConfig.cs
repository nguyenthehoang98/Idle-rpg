using Sirenix.OdinInspector;

namespace _KITSystem.SkillSystem.Config
{
    public class CannonTrajectoryConfig : BaseTrajectoryConfig
    {         
        [TitleGroup("Cannon")]
        public float initialSpeed;
        public float gravity;
        public float delay;
        public override TrajectoryType Type => TrajectoryType.Cannon;
    }
}