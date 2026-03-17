using UnityEngine;

namespace _Games.Combat.SkillSystem.Config
{
    [CreateAssetMenu(fileName = "Modifier", menuName = "Game/Skill/Modifier-KnockBack")]
    public class KnockBackModifierSO : BaseModifierSO
    {
        public float duration;
        public float distance;
        public AnimationCurve curve;
    }
}