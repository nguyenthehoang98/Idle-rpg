using UnityEngine;

namespace _Games.Combat.SkillSystem.Config
{
    [System.Serializable]
    public abstract class BaseTrajectorySO : ScriptableObject
    {
        public abstract TrajectoryType Type { get; }
    }

    public enum TrajectoryType
    {
        Curve, Path
    }
}