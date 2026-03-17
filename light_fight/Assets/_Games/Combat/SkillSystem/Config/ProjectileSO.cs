using UnityEngine;

namespace _Games.Combat.SkillSystem.Config
{
    [CreateAssetMenu(fileName = "Projectile", menuName = "Game/Skill/Projectile")]
    public class ProjectileSO : ScriptableObject
    {
        public GameObject prefab;
    }
}