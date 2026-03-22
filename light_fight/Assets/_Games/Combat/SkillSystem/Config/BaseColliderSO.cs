using UnityEngine;

namespace _Games.Combat.SkillSystem.Config
{
    [System.Serializable]
    public abstract class BaseColliderSO : ScriptableObject
    {
        public abstract ColliderType Type { get; }
    }

    public enum ColliderType
    {
        Circle,
    }
}