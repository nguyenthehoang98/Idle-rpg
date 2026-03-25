using UnityEngine;

namespace _Games.Combat.SkillSystem.Config
{
    [CreateAssetMenu(fileName = "Path", menuName = "Game-Skill/Trajectory-Path")]
    public class PathTrajectorySO : BaseTrajectorySO
    {
        public override TrajectoryType Type => TrajectoryType.Path;
    }
}