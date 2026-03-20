using UnityEngine;

namespace _Games.Combat.SkillSystem.Config
{
    [CreateAssetMenu(fileName = "Curve", menuName = "Game/Skill/Trajectory-Curve")]
    public class CurveTrajectorySO : BaseTrajectorySO
    {
        public AnimationCurve curve;
        public float value;
        public override TrajectoryType Type => TrajectoryType.Curve;
    }
}