using UnityEngine;

namespace _Games.Combat.SkillSystem.Config
{
    [CreateAssetMenu(fileName = "Curve", menuName = "Game-Skill/Trajectory/Parabolic")]
    public class ParabolicTrajectorySO : BaseTrajectorySO
    {
        [Tooltip("Curve thể hiện cho việc thay đổi độ cao vật thể bay")]
        public AnimationCurve curve;
        public float height;
        public float speed;
        
        public override TrajectoryType Type => TrajectoryType.Parabolic;
    }
}