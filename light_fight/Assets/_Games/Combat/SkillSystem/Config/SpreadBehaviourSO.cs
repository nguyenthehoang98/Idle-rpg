using UnityEngine;

namespace _Games.Combat.SkillSystem.Config
{
    [CreateAssetMenu(fileName = "Behavior", menuName = "Game-Skill/Behavior/Spread")]
    public class SpreadBehaviourSO : BaseBehaviorSO
    {
        public int count;
        public float angleStep;
        public float delayBetween;
        public bool randomAngle;
        public override BehaviourType Type => BehaviourType.Spread;
    }
}