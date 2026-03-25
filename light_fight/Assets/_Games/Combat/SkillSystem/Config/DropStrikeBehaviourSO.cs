using UnityEngine;

namespace _Games.Combat.SkillSystem.Config
{
    [CreateAssetMenu(fileName = "Behavior", menuName = "Game-Skill/Behavior-DropStrike")]
    public class DropStrikeBehaviourSO : BaseBehaviorSO
    {
        public int count;
        public float height;
        public float delayBetween;
        public float radius;
        public bool overrideCenter;
        public float angle;
        public override BehaviourType Type => BehaviourType.DropStrike;
    }
}