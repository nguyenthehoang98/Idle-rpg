using UnityEngine;

namespace _Games.Combat.SkillSystem.Config
{
    [CreateAssetMenu(fileName = "Boomerang", menuName = "Game-Skill/Trajectory/Boomerang")]
    public class BoomerangTrajectorySO : BaseTrajectorySO
    {
        public AnimationCurve castPhase;
        public float castMaxSpeed;
        public AnimationCurve returnPhase;
        public float returnMaxSpeed;
        public override TrajectoryType Type => TrajectoryType.Boomerang;
    }
}