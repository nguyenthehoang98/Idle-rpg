using Sirenix.OdinInspector;
using UnityEngine;

namespace _KITSystem.SkillSystem
{
    /// <summary>
    /// Lớp này có thể override data từ excel. override trực tiếp luôn
    /// </summary>
    [CreateAssetMenu(fileName = "New Skill", menuName = "Game/Skill")]
    public sealed class Skill : SerializedScriptableObject
    {
        public Trigger trigger;
        public Projectile projectile;
        [SerializeReference] public BaseCollider collider;
        [SerializeReference] public BaseBehaviour behaviour;
        [SerializeReference] public BaseModifier modifier;
    }
}
