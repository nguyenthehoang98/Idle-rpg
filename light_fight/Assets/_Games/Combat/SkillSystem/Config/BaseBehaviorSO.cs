using UnityEngine;

namespace _Games.Combat.SkillSystem.Config
{
    [System.Serializable]
    public abstract class BaseBehaviorSO : ScriptableObject
    {
        public abstract BehaviourType Type { get; }
    }

    public enum BehaviourType
    {
        Spread, DropStrike, Explosion, Piercing, Parallel,
        Bounce
    }
}