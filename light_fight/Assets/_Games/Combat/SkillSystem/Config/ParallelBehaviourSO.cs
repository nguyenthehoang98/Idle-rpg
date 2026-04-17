using UnityEngine;

namespace _Games.Combat.SkillSystem.Config
{
    [CreateAssetMenu(fileName = "Behavior", menuName = "Game-Skill/Behavior-Parallel")]
    public class ParallelBehaviourSO : BaseBehaviorSO
    {
        public int count;
        public Vector2 centerOffset;
        public float distanceBetween;
        public override BehaviourType Type => BehaviourType.Parallel;
    }
}