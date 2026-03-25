using UnityEngine;

namespace _Games.Combat.SkillSystem.Config
{
    [CreateAssetMenu(fileName = "Modifier", menuName = "Game-Skill/Modifier-Slow")]
    public class SlowModifierSO : BaseModifierSO
    {
        public float duration;
        public float value;
    }
}