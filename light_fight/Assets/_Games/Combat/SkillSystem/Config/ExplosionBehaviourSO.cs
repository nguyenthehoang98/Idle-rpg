using UnityEngine;

namespace _Games.Combat.SkillSystem.Config
{
    [CreateAssetMenu(fileName = "Behavior", menuName = "Game/Skill/Behavior-Explosion")]
    public class ExplosionBehaviourSO : BaseBehaviorSO
    {
        public float radius;
        public Vector3 offset;
        public override BehaviourType Type => BehaviourType.Explosion;
    }
}