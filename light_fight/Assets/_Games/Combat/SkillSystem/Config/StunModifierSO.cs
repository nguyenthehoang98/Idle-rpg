using UnityEngine;

namespace _Games.Combat.SkillSystem.Config
{
    [CreateAssetMenu(fileName = "Modifier", menuName = "Game/Skill/Modifier-Stun")]
    public class StunModifierSO : BaseModifierSO
    {
        public float duration;
    }
}