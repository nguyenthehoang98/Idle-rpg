using UnityEngine;

namespace _Games.Combat.SkillSystem.Config
{
    [CreateAssetMenu(fileName = "Curve", menuName = "Game-Skill/Trajectory/Curve")]
    public class CurveTrajectorySO : BaseTrajectorySO
    {
        [Tooltip("Curve thể hiện cho việc thay đổi tốc độ vật thể bay")]
        public AnimationCurve curve;
        public float maxSpeed;
        public override TrajectoryType Type => TrajectoryType.Curve;
    }
}