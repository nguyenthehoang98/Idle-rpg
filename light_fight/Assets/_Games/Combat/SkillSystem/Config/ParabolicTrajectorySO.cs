using UnityEngine;

namespace _Games.Combat.SkillSystem.Config
{
    [CreateAssetMenu(fileName = "Curve", menuName = "Game-Skill/Trajectory/Parabolic")]
    public class ParabolicTrajectorySO : BaseTrajectorySO
    {
        [Tooltip("Curve thể hiện cho việc thay đổi độ cao vật thể bay")]
        public AnimationCurve curve;
        public float minHeight;
        public float maxHeight;
        public float minDistance;
        public float maxDistance;
        public float minSpeed;
        public float maxSpeed;
        
        public override TrajectoryType Type => TrajectoryType.Parabolic;
    }
}